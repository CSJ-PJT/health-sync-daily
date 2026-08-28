-- Remove the legacy permissive baseline. Direct anonymous table access is not
-- required by Fifth Dawn: its anonymous clients use the token-scoped RPCs.

revoke all on table
  public.commerce_entitlements,
  public.commerce_products,
  public.commerce_purchases,
  public.entertainment_challenges,
  public.entertainment_life_sim_saves,
  public.entertainment_rooms,
  public.entertainment_score_events,
  public.entertainment_scores,
  public.entertainment_strategy_events,
  public.entertainment_strategy_matches,
  public.entertainment_strategy_season_scores,
  public.entertainment_strategy_snapshots,
  public.entertainment_world_events,
  public.entertainment_world_permissions,
  public.entertainment_world_reactions,
  public.entertainment_world_snapshots,
  public.entertainment_worlds,
  public.fifth_dawn_unlock_mappings,
  public.game_account_links,
  public.game_link_missions,
  public.game_link_profiles,
  public.game_link_rewards,
  public.life_sim_player_states,
  public.openai_credentials,
  public.transfer_logs
from public, anon;

grant select on table public.commerce_products, public.fifth_dawn_unlock_mappings to authenticated;
grant select on table public.commerce_entitlements, public.commerce_purchases to authenticated;

grant select, insert, update, delete on table
  public.entertainment_challenges,
  public.entertainment_life_sim_saves,
  public.entertainment_rooms,
  public.entertainment_scores,
  public.openai_credentials,
  public.transfer_logs
to authenticated;

grant select, insert on table public.entertainment_score_events to authenticated;
grant select, insert, update on table
  public.entertainment_strategy_matches,
  public.entertainment_strategy_snapshots,
  public.entertainment_strategy_season_scores,
  public.entertainment_worlds,
  public.entertainment_world_snapshots
to authenticated;
grant select, insert on table
  public.entertainment_strategy_events,
  public.entertainment_world_events
to authenticated;
grant select, insert, delete on table public.entertainment_world_reactions to authenticated;
grant select, insert, update, delete on table public.entertainment_world_permissions to authenticated;

do $policy_cleanup$
declare
  policy_row record;
begin
  for policy_row in
    select schemaname, tablename, policyname
    from pg_policies
    where schemaname = 'public'
      and 'public' = any(roles)
      and tablename::text = any(array[
        'entertainment_challenges',
        'entertainment_life_sim_saves',
        'entertainment_rooms',
        'entertainment_score_events',
        'entertainment_scores',
        'entertainment_strategy_events',
        'entertainment_strategy_matches',
        'entertainment_strategy_season_scores',
        'entertainment_strategy_snapshots',
        'entertainment_world_events',
        'entertainment_world_permissions',
        'entertainment_world_reactions',
        'entertainment_world_snapshots',
        'entertainment_worlds',
        'openai_credentials',
        'transfer_logs'
      ])
  loop
    execute format('drop policy if exists %I on %I.%I', policy_row.policyname, policy_row.schemaname, policy_row.tablename);
  end loop;
end
$policy_cleanup$;

create policy entertainment_challenges_owner_all
  on public.entertainment_challenges for all to authenticated
  using (exists (
    select 1 from public.profiles p
    where p.id = entertainment_challenges.profile_id
      and p.user_id = (select auth.uid())::text
  ))
  with check (exists (
    select 1 from public.profiles p
    where p.id = entertainment_challenges.profile_id
      and p.user_id = (select auth.uid())::text
  ));

create policy entertainment_life_sim_saves_owner_all
  on public.entertainment_life_sim_saves for all to authenticated
  using (exists (
    select 1 from public.profiles p
    where p.id = entertainment_life_sim_saves.profile_id
      and p.user_id = (select auth.uid())::text
  ))
  with check (exists (
    select 1 from public.profiles p
    where p.id = entertainment_life_sim_saves.profile_id
      and p.user_id = (select auth.uid())::text
  ));

create policy entertainment_rooms_owner_all
  on public.entertainment_rooms for all to authenticated
  using (exists (
    select 1 from public.profiles p
    where p.id = entertainment_rooms.profile_id
      and p.user_id = (select auth.uid())::text
  ))
  with check (exists (
    select 1 from public.profiles p
    where p.id = entertainment_rooms.profile_id
      and p.user_id = (select auth.uid())::text
  ));

create policy entertainment_scores_owner_all
  on public.entertainment_scores for all to authenticated
  using (exists (
    select 1 from public.profiles p
    where p.id = entertainment_scores.profile_id
      and p.user_id = (select auth.uid())::text
  ))
  with check (exists (
    select 1 from public.profiles p
    where p.id = entertainment_scores.profile_id
      and p.user_id = (select auth.uid())::text
  ));

create policy entertainment_score_events_authenticated_read
  on public.entertainment_score_events for select to authenticated
  using (true);

create policy entertainment_score_events_owner_insert
  on public.entertainment_score_events for insert to authenticated
  with check (exists (
    select 1 from public.profiles p
    where p.id = entertainment_score_events.profile_id
      and p.user_id = (select auth.uid())::text
  ));

create policy entertainment_strategy_matches_owner_all
  on public.entertainment_strategy_matches for all to authenticated
  using (exists (
    select 1 from public.profiles p
    where p.id = entertainment_strategy_matches.profile_id
      and p.user_id = (select auth.uid())::text
  ))
  with check (exists (
    select 1 from public.profiles p
    where p.id = entertainment_strategy_matches.profile_id
      and p.user_id = (select auth.uid())::text
  ));

create policy entertainment_strategy_events_match_owner_all
  on public.entertainment_strategy_events for all to authenticated
  using (exists (
    select 1
    from public.entertainment_strategy_matches m
    join public.profiles p on p.id = m.profile_id
    where m.id = entertainment_strategy_events.match_id
      and p.user_id = (select auth.uid())::text
  ))
  with check (exists (
    select 1
    from public.entertainment_strategy_matches m
    join public.profiles p on p.id = m.profile_id
    where m.id = entertainment_strategy_events.match_id
      and p.user_id = (select auth.uid())::text
  ));

create policy entertainment_strategy_snapshots_match_owner_all
  on public.entertainment_strategy_snapshots for all to authenticated
  using (exists (
    select 1
    from public.entertainment_strategy_matches m
    join public.profiles p on p.id = m.profile_id
    where m.id = entertainment_strategy_snapshots.match_id
      and p.user_id = (select auth.uid())::text
  ))
  with check (exists (
    select 1
    from public.entertainment_strategy_matches m
    join public.profiles p on p.id = m.profile_id
    where m.id = entertainment_strategy_snapshots.match_id
      and p.user_id = (select auth.uid())::text
  ));

create policy entertainment_strategy_season_scores_authenticated_read
  on public.entertainment_strategy_season_scores for select to authenticated
  using (true);

create policy entertainment_strategy_season_scores_owner_write
  on public.entertainment_strategy_season_scores for insert to authenticated
  with check (user_id = (select auth.uid())::text);

create policy entertainment_strategy_season_scores_owner_update
  on public.entertainment_strategy_season_scores for update to authenticated
  using (user_id = (select auth.uid())::text)
  with check (user_id = (select auth.uid())::text);

create policy entertainment_worlds_authenticated_read
  on public.entertainment_worlds for select to authenticated
  using (visibility = 'public' or owner_user_id = (select auth.uid())::text);

create policy entertainment_worlds_owner_insert
  on public.entertainment_worlds for insert to authenticated
  with check (owner_user_id = (select auth.uid())::text);

create policy entertainment_worlds_owner_update
  on public.entertainment_worlds for update to authenticated
  using (owner_user_id = (select auth.uid())::text)
  with check (owner_user_id = (select auth.uid())::text);

create policy entertainment_world_snapshots_visible_read
  on public.entertainment_world_snapshots for select to authenticated
  using (exists (
    select 1 from public.entertainment_worlds w
    where w.id = entertainment_world_snapshots.world_id
      and (w.visibility = 'public' or w.owner_user_id = (select auth.uid())::text)
  ));

create policy entertainment_world_snapshots_owner_write
  on public.entertainment_world_snapshots for insert to authenticated
  with check (exists (
    select 1 from public.entertainment_worlds w
    where w.id = entertainment_world_snapshots.world_id
      and w.owner_user_id = (select auth.uid())::text
  ));

create policy entertainment_world_snapshots_owner_update
  on public.entertainment_world_snapshots for update to authenticated
  using (exists (
    select 1 from public.entertainment_worlds w
    where w.id = entertainment_world_snapshots.world_id
      and w.owner_user_id = (select auth.uid())::text
  ))
  with check (exists (
    select 1 from public.entertainment_worlds w
    where w.id = entertainment_world_snapshots.world_id
      and w.owner_user_id = (select auth.uid())::text
  ));

create policy entertainment_world_events_visible_read
  on public.entertainment_world_events for select to authenticated
  using (exists (
    select 1 from public.entertainment_worlds w
    where w.id = entertainment_world_events.world_id
      and (w.visibility = 'public' or w.owner_user_id = (select auth.uid())::text)
  ));

create policy entertainment_world_events_owner_insert
  on public.entertainment_world_events for insert to authenticated
  with check (exists (
    select 1 from public.entertainment_worlds w
    where w.id = entertainment_world_events.world_id
      and w.owner_user_id = (select auth.uid())::text
  ));

create policy entertainment_world_reactions_visible_read
  on public.entertainment_world_reactions for select to authenticated
  using (exists (
    select 1 from public.entertainment_worlds w
    where w.id = entertainment_world_reactions.world_id
      and (w.visibility = 'public' or w.owner_user_id = (select auth.uid())::text)
  ));

create policy entertainment_world_reactions_owner_write
  on public.entertainment_world_reactions for insert to authenticated
  with check (user_id = (select auth.uid())::text);

create policy entertainment_world_reactions_owner_delete
  on public.entertainment_world_reactions for delete to authenticated
  using (user_id = (select auth.uid())::text);

create policy entertainment_world_permissions_owner_all
  on public.entertainment_world_permissions for all to authenticated
  using (exists (
    select 1 from public.entertainment_worlds w
    where w.id = entertainment_world_permissions.world_id
      and w.owner_user_id = (select auth.uid())::text
  ))
  with check (exists (
    select 1 from public.entertainment_worlds w
    where w.id = entertainment_world_permissions.world_id
      and w.owner_user_id = (select auth.uid())::text
  ));

create policy openai_credentials_owner_all
  on public.openai_credentials for all to authenticated
  using (exists (
    select 1 from public.profiles p
    where p.id = openai_credentials.profile_id
      and p.user_id = (select auth.uid())::text
  ))
  with check (exists (
    select 1 from public.profiles p
    where p.id = openai_credentials.profile_id
      and p.user_id = (select auth.uid())::text
  ));

create policy transfer_logs_owner_all
  on public.transfer_logs for all to authenticated
  using (exists (
    select 1 from public.profiles p
    where p.id = transfer_logs.profile_id
      and p.user_id = (select auth.uid())::text
  ))
  with check (exists (
    select 1 from public.profiles p
    where p.id = transfer_logs.profile_id
      and p.user_id = (select auth.uid())::text
  ));
