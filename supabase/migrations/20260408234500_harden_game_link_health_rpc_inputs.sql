create or replace function public.refresh_game_link_profile(
  target_profile_id uuid,
  target_user_id text
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
  verified_profile_id uuid;
begin
  select id
    into verified_profile_id
  from public.profiles
  where id = target_profile_id
    and user_id = target_user_id
  limit 1;

  if verified_profile_id is null then
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
set search_path = public
as $$
begin
  if not exists (
    select 1
    from public.profiles
    where id = target_profile_id
      and user_id = target_user_id
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
set search_path = public
as $$
declare
  verified_profile_id uuid;
begin
  select id
    into verified_profile_id
  from public.profiles
  where id = target_profile_id
    and user_id = target_user_id
  limit 1;

  if verified_profile_id is null then
    raise exception 'profile_user_mismatch';
  end if;

  return public.fetch_health_game_link_bundle(target_profile_id);
end;
$$;

grant execute on function public.refresh_game_link_profile(uuid, text) to anon, authenticated;
grant execute on function public.disconnect_game_account(uuid, text) to anon, authenticated;
grant execute on function public.fetch_health_game_link_bundle(uuid, text) to anon, authenticated;
