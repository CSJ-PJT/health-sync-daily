-- Close the legacy anonymous data plane before it receives production data.
-- These tables are currently empty; future rows are owned by the signed-in
-- Supabase user through profiles.user_id = auth.uid()::text.

drop policy if exists "Users can insert their own profile" on public.profiles;
drop policy if exists "Users can update their own profile" on public.profiles;
drop policy if exists "Users can view their own profile" on public.profiles;

alter table public.profiles enable row level security;
revoke all on table public.profiles from public, anon;
grant select, insert, update, delete on table public.profiles to authenticated;

create policy profiles_owner_select
on public.profiles
for select
to authenticated
using (user_id = (select auth.uid())::text);

create policy profiles_owner_insert
on public.profiles
for insert
to authenticated
with check (user_id = (select auth.uid())::text);

create policy profiles_owner_update
on public.profiles
for update
to authenticated
using (user_id = (select auth.uid())::text)
with check (user_id = (select auth.uid())::text);

create policy profiles_owner_delete
on public.profiles
for delete
to authenticated
using (user_id = (select auth.uid())::text);

create index if not exists profiles_user_id_idx on public.profiles(user_id);

do $migration$
declare
  target_table text;
  owned_tables constant text[] := array[
    'app_state_snapshots',
    'social_feed_posts',
    'social_feed_comments',
    'social_friends',
    'social_chat_rooms',
    'social_chat_messages',
    'user_earned_badges',
    'user_verified_records',
    'user_profile_preferences',
    'user_profile_settings',
    'app_audit_events',
    'data_subject_requests'
  ];
begin
  foreach target_table in array owned_tables loop
    execute format('alter table public.%I enable row level security', target_table);
    execute format('revoke all on table public.%I from public, anon', target_table);
    execute format('grant select, insert, update, delete on table public.%I to authenticated', target_table);
    execute format('drop policy if exists %I on public.%I', target_table || '_owner_all', target_table);
    execute format(
      'create policy %I on public.%I for all to authenticated '
      || 'using (exists (select 1 from public.profiles p '
      || 'where p.id = profile_id and p.user_id = (select auth.uid())::text)) '
      || 'with check (exists (select 1 from public.profiles p '
      || 'where p.id = profile_id and p.user_id = (select auth.uid())::text))',
      target_table || '_owner_all',
      target_table
    );
  end loop;
end
$migration$;

-- Health-side link operations require a real Supabase identity. The legacy
-- profile/user-id pair alone was not authentication.
create or replace function public.connect_game_account(
  target_profile_id uuid,
  target_user_id text,
  requested_game_account_id text
)
returns jsonb
language plpgsql
security definer
set search_path = ''
as $$
declare
  generated_token text := encode(extensions.gen_random_bytes(24), 'hex');
begin
  if auth.uid() is null or target_user_id <> auth.uid()::text then
    raise exception 'authentication_required';
  end if;

  if not exists (
    select 1
    from public.profiles
    where id = target_profile_id
      and user_id = auth.uid()::text
  ) then
    raise exception 'profile_not_found';
  end if;

  insert into public.game_account_links (
    profile_id,
    user_id,
    game_account_id,
    link_token,
    link_status,
    updated_at,
    disconnected_at
  )
  values (
    target_profile_id,
    target_user_id,
    requested_game_account_id,
    generated_token,
    'linked',
    now(),
    null
  )
  on conflict (profile_id) do update
    set user_id = excluded.user_id,
        game_account_id = excluded.game_account_id,
        link_token = excluded.link_token,
        link_status = 'linked',
        updated_at = now(),
        disconnected_at = null;

  perform public.refresh_game_link_profile(target_profile_id);

  return jsonb_build_object(
    'linkToken', generated_token,
    'linkStatus', 'linked'
  );
end;
$$;

create or replace function public.refresh_game_link_profile(
  target_profile_id uuid,
  target_user_id text
)
returns jsonb
language plpgsql
security definer
set search_path = ''
as $$
begin
  if auth.uid() is null or target_user_id <> auth.uid()::text then
    raise exception 'authentication_required';
  end if;
  if not exists (
    select 1 from public.profiles
    where id = target_profile_id and user_id = auth.uid()::text
  ) then
    raise exception 'profile_user_mismatch';
  end if;
  return public.refresh_game_link_profile(target_profile_id);
end;
$$;

create or replace function public.disconnect_game_account(
  target_profile_id uuid,
  target_user_id text
)
returns void
language plpgsql
security definer
set search_path = ''
as $$
begin
  if auth.uid() is null or target_user_id <> auth.uid()::text then
    raise exception 'authentication_required';
  end if;
  if not exists (
    select 1 from public.profiles
    where id = target_profile_id and user_id = auth.uid()::text
  ) then
    raise exception 'profile_user_mismatch';
  end if;
  update public.game_account_links
    set link_status = 'paused',
        disconnected_at = now(),
        updated_at = now()
  where profile_id = target_profile_id;
end;
$$;

create or replace function public.fetch_health_game_link_bundle(
  target_profile_id uuid,
  target_user_id text
)
returns jsonb
language plpgsql
security definer
set search_path = ''
as $$
begin
  if auth.uid() is null or target_user_id <> auth.uid()::text then
    raise exception 'authentication_required';
  end if;
  if not exists (
    select 1 from public.profiles
    where id = target_profile_id and user_id = auth.uid()::text
  ) then
    raise exception 'profile_user_mismatch';
  end if;
  return public.fetch_health_game_link_bundle(target_profile_id);
end;
$$;

-- Revoke the implicit PUBLIC execute grant from every exposed definer helper.
-- Re-grant only the current, credential-scoped product entry points.
revoke execute on function public.connect_game_account(uuid, text, text) from public, anon, authenticated;
revoke execute on function public.disconnect_game_account(uuid) from public, anon, authenticated;
revoke execute on function public.disconnect_game_account(uuid, text) from public, anon, authenticated;
revoke execute on function public.refresh_game_link_profile(uuid) from public, anon, authenticated;
revoke execute on function public.refresh_game_link_profile(uuid, text) from public, anon, authenticated;
revoke execute on function public.fetch_health_game_link_bundle(uuid) from public, anon, authenticated;
revoke execute on function public.fetch_health_game_link_bundle(uuid, text) from public, anon, authenticated;
revoke execute on function public.fetch_game_link_bundle(text) from public, anon, authenticated;
revoke execute on function public.fetch_game_link_bundle(text, text) from public, anon, authenticated;
revoke execute on function public.fetch_game_link_bundle(text, text, text) from public, anon, authenticated;
revoke execute on function public.fetch_life_sim_state(text, text) from public, anon, authenticated;
revoke execute on function public.fetch_life_sim_state(text, text, text) from public, anon, authenticated;
revoke execute on function public.fetch_life_sim_state(text, text, text, text) from public, anon, authenticated;
revoke execute on function public.upsert_life_sim_state(text, text, jsonb) from public, anon, authenticated;
revoke execute on function public.upsert_life_sim_state(text, text, text, jsonb) from public, anon, authenticated;
revoke execute on function public.upsert_life_sim_state(text, text, text, text, jsonb) from public, anon, authenticated;
revoke execute on function public.update_updated_at_column() from public, anon, authenticated;

grant execute on function public.connect_game_account(uuid, text, text) to authenticated;
grant execute on function public.disconnect_game_account(uuid, text) to authenticated;
grant execute on function public.refresh_game_link_profile(uuid, text) to authenticated;
grant execute on function public.fetch_health_game_link_bundle(uuid, text) to authenticated;

grant execute on function public.fetch_game_link_bundle(text, text, text) to anon, authenticated;
grant execute on function public.fetch_life_sim_state(text, text, text, text) to anon, authenticated;
grant execute on function public.upsert_life_sim_state(text, text, text, text, jsonb) to anon, authenticated;
