create or replace function public.fetch_game_link_bundle(
  supplied_link_token text,
  supplied_game_account_id text
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
  link_row public.game_account_links%rowtype;
  profile_row public.game_link_profiles%rowtype;
begin
  select * into link_row
  from public.game_account_links
  where link_token = supplied_link_token
    and game_account_id = supplied_game_account_id
    and link_status = 'linked'
  limit 1;

  if not found then
    return null;
  end if;

  select * into profile_row
  from public.game_link_profiles
  where link_id = link_row.id
  limit 1;

  return jsonb_build_object(
    'accountLink',
    jsonb_build_object(
      'profileId', link_row.profile_id,
      'userId', link_row.user_id,
      'gameAccountId', link_row.game_account_id,
      'linkStatus', link_row.link_status,
      'linkToken', null,
      'linkedAt', link_row.created_at
    ),
    'profile',
    case
      when profile_row.id is null then null
      else jsonb_build_object(
        'activityTier', profile_row.activity_tier,
        'sleepTier', profile_row.sleep_tier,
        'recoveryTier', profile_row.recovery_tier,
        'hydrationTier', profile_row.hydration_tier,
        'consistencyScore', profile_row.consistency_score,
        'weeklyMovementScore', profile_row.weekly_movement_score,
        'focusScore', profile_row.focus_score,
        'resonancePoints', profile_row.resonance_points,
        'dailyMissionFlags', profile_row.daily_mission_flags,
        'weeklyMissionFlags', profile_row.weekly_mission_flags,
        'lastRefreshAt', profile_row.last_refresh_at
      )
    end,
    'missions',
    coalesce(
      (
        select jsonb_agg(
          jsonb_build_object(
            'id', mission.id,
            'missionScope', mission.mission_scope,
            'missionKey', mission.mission_key,
            'title', mission.title,
            'description', mission.description,
            'status', mission.status
          )
          order by mission.created_at desc
        )
        from public.game_link_missions mission
        where mission.link_id = link_row.id
      ),
      '[]'::jsonb
    ),
    'rewards',
    coalesce(
      (
        select jsonb_agg(
          jsonb_build_object(
            'id', reward.id,
            'rewardKey', reward.reward_key,
            'rewardType', reward.reward_type,
            'payload', reward.reward_payload,
            'grantedAt', reward.granted_at,
            'claimedAt', reward.claimed_at
          )
          order by reward.granted_at desc
        )
        from public.game_link_rewards reward
        where reward.link_id = link_row.id
      ),
      '[]'::jsonb
    )
  );
end;
$$;

create or replace function public.fetch_life_sim_state(
  supplied_link_token text,
  supplied_game_account_id text,
  requested_slot text default 'main'
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
  link_row public.game_account_links%rowtype;
  save_row public.life_sim_player_states%rowtype;
begin
  select * into link_row
  from public.game_account_links
  where link_token = supplied_link_token
    and game_account_id = supplied_game_account_id
    and link_status = 'linked'
  limit 1;

  if not found then
    return null;
  end if;

  select * into save_row
  from public.life_sim_player_states
  where link_id = link_row.id
    and slot = requested_slot
  limit 1;

  if save_row.id is null then
    return null;
  end if;

  return save_row.state;
end;
$$;

create or replace function public.upsert_life_sim_state(
  supplied_link_token text,
  supplied_game_account_id text,
  requested_slot text,
  requested_state jsonb
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
  link_row public.game_account_links%rowtype;
  saved_row public.life_sim_player_states%rowtype;
begin
  select * into link_row
  from public.game_account_links
  where link_token = supplied_link_token
    and game_account_id = supplied_game_account_id
    and link_status = 'linked'
  limit 1;

  if not found then
    raise exception 'invalid_link_credentials';
  end if;

  insert into public.life_sim_player_states (link_id, slot, state, updated_at)
  values (link_row.id, requested_slot, requested_state, now())
  on conflict (link_id, slot) do update
    set state = excluded.state,
        updated_at = now()
  returning * into saved_row;

  return saved_row.state;
end;
$$;

revoke execute on function public.fetch_game_link_bundle(text) from anon, authenticated;
revoke execute on function public.fetch_life_sim_state(text, text) from anon, authenticated;
revoke execute on function public.upsert_life_sim_state(text, text, jsonb) from anon, authenticated;

grant execute on function public.fetch_game_link_bundle(text, text) to anon, authenticated;
grant execute on function public.fetch_life_sim_state(text, text, text) to anon, authenticated;
grant execute on function public.upsert_life_sim_state(text, text, text, jsonb) to anon, authenticated;
