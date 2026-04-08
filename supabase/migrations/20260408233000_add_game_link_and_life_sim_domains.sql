create extension if not exists pgcrypto;

create table if not exists public.game_account_links (
  id uuid primary key default gen_random_uuid(),
  profile_id uuid not null references public.profiles(id) on delete cascade,
  user_id text not null,
  game_account_id text not null,
  link_token text not null unique,
  link_status text not null default 'linked',
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  disconnected_at timestamptz null,
  unique (profile_id)
);

create table if not exists public.game_link_profiles (
  id uuid primary key default gen_random_uuid(),
  link_id uuid not null references public.game_account_links(id) on delete cascade,
  profile_id uuid not null references public.profiles(id) on delete cascade,
  user_id text not null,
  activity_tier integer not null default 0,
  sleep_tier integer not null default 0,
  recovery_tier integer not null default 0,
  hydration_tier integer not null default 0,
  consistency_score integer not null default 0,
  weekly_movement_score integer not null default 0,
  focus_score integer not null default 0,
  resonance_points integer not null default 0,
  daily_mission_flags jsonb not null default '[]'::jsonb,
  weekly_mission_flags jsonb not null default '[]'::jsonb,
  last_refresh_at timestamptz null,
  updated_at timestamptz not null default now(),
  unique (link_id)
);

create table if not exists public.game_link_missions (
  id uuid primary key default gen_random_uuid(),
  link_id uuid not null references public.game_account_links(id) on delete cascade,
  mission_scope text not null,
  mission_key text not null,
  title text not null,
  description text not null,
  status text not null,
  reward_key text not null,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create table if not exists public.game_link_rewards (
  id uuid primary key default gen_random_uuid(),
  link_id uuid not null references public.game_account_links(id) on delete cascade,
  reward_key text not null,
  reward_type text not null,
  reward_payload jsonb not null default '{}'::jsonb,
  granted_at timestamptz not null default now(),
  claimed_at timestamptz null
);

create table if not exists public.life_sim_player_states (
  id uuid primary key default gen_random_uuid(),
  link_id uuid null references public.game_account_links(id) on delete cascade,
  slot text not null,
  state jsonb not null,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  unique (link_id, slot)
);

create index if not exists idx_game_account_links_profile_id on public.game_account_links(profile_id);
create index if not exists idx_game_account_links_user_id on public.game_account_links(user_id);
create index if not exists idx_game_link_profiles_link_id on public.game_link_profiles(link_id);
create index if not exists idx_game_link_missions_link_id on public.game_link_missions(link_id, mission_scope);
create index if not exists idx_game_link_rewards_link_id on public.game_link_rewards(link_id);
create index if not exists idx_life_sim_player_states_link_slot on public.life_sim_player_states(link_id, slot);

alter table public.game_account_links enable row level security;
alter table public.game_link_profiles enable row level security;
alter table public.game_link_missions enable row level security;
alter table public.game_link_rewards enable row level security;
alter table public.life_sim_player_states enable row level security;

comment on table public.game_account_links is 'Shared identity bridge between the health app and the standalone Fifth Dawn game.';
comment on table public.game_link_profiles is 'Derived game-safe parameters only. No raw health records are stored here.';
comment on table public.life_sim_player_states is 'Optional cloud save state for linked standalone game accounts.';

create or replace function public.refresh_game_link_profile(target_profile_id uuid)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
  link_row public.game_account_links%rowtype;
  source_user_id text;
  avg_steps numeric := 0;
  avg_sleep numeric := 0;
  avg_water numeric := 0;
  run_sessions integer := 0;
  high_step_days integer := 0;
  activity_tier integer := 0;
  sleep_tier integer := 0;
  recovery_tier integer := 0;
  hydration_tier integer := 0;
  consistency_score integer := 0;
  weekly_movement_score integer := 0;
  focus_score integer := 0;
  resonance_points integer := 0;
  daily_flags jsonb := '[]'::jsonb;
  weekly_flags jsonb := '[]'::jsonb;
begin
  select *
    into link_row
  from public.game_account_links
  where profile_id = target_profile_id
    and link_status = 'linked'
  limit 1;

  if not found then
    return null;
  end if;

  select user_id into source_user_id
  from public.profiles
  where id = target_profile_id
  limit 1;

  if source_user_id is null then
    return null;
  end if;

  with recent as (
    select *
    from public.health_data
    where user_id = source_user_id
    order by synced_at desc
    limit 14
  )
  select
    coalesce(avg(nullif(coalesce((steps_data->>'count')::numeric, (steps_data->>'steps')::numeric, 0), 0)), 0),
    coalesce(avg(nullif(coalesce((sleep_data->>'totalMinutes')::numeric, 0), 0)), 0),
    coalesce(avg(nullif(coalesce((nutrition_data->>'waterIntakeMl')::numeric, (nutrition_data->>'waterMl')::numeric, 0), 0)), 0),
    count(*) filter (where coalesce((running_data->'summary'->>'distanceKm')::numeric, 0) > 0),
    count(*) filter (where coalesce((steps_data->>'count')::numeric, (steps_data->>'steps')::numeric, 0) >= 6000)
  into avg_steps, avg_sleep, avg_water, run_sessions, high_step_days
  from recent;

  activity_tier :=
    case
      when avg_steps >= 11000 or run_sessions >= 6 then 3
      when avg_steps >= 8500 or run_sessions >= 4 then 2
      when avg_steps >= 5500 or run_sessions >= 2 then 1
      else 0
    end;

  sleep_tier :=
    case
      when avg_sleep >= 450 then 3
      when avg_sleep >= 400 then 2
      when avg_sleep >= 350 then 1
      else 0
    end;

  recovery_tier :=
    case
      when sleep_tier >= 3 and activity_tier >= 2 then 3
      when sleep_tier >= 2 then 2
      when sleep_tier >= 1 then 1
      else 0
    end;

  hydration_tier :=
    case
      when avg_water >= 2200 then 3
      when avg_water >= 1700 then 2
      when avg_water >= 1200 then 1
      else 0
    end;

  consistency_score := least(100, greatest(0, round((high_step_days::numeric / 7.0) * 100)));
  weekly_movement_score := least(100, greatest(0, round((avg_steps / 12000.0) * 70 + run_sessions * 5)));
  focus_score := least(100, greatest(0, round((sleep_tier * 20) + (hydration_tier * 10) + (activity_tier * 10))));
  resonance_points := activity_tier * 8 + sleep_tier * 7 + recovery_tier * 6 + hydration_tier * 5 + round(consistency_score / 5.0);

  if avg_steps >= 8000 then
    daily_flags := daily_flags || jsonb_build_array('daily-steps');
  end if;
  if avg_water >= 1800 then
    daily_flags := daily_flags || jsonb_build_array('daily-hydration');
  end if;
  if consistency_score >= 70 then
    weekly_flags := weekly_flags || jsonb_build_array('weekly-consistency');
  end if;
  if run_sessions >= 3 then
    weekly_flags := weekly_flags || jsonb_build_array('weekly-movement');
  end if;

  insert into public.game_link_profiles (
    link_id,
    profile_id,
    user_id,
    activity_tier,
    sleep_tier,
    recovery_tier,
    hydration_tier,
    consistency_score,
    weekly_movement_score,
    focus_score,
    resonance_points,
    daily_mission_flags,
    weekly_mission_flags,
    last_refresh_at,
    updated_at
  )
  values (
    link_row.id,
    link_row.profile_id,
    link_row.user_id,
    activity_tier,
    sleep_tier,
    recovery_tier,
    hydration_tier,
    consistency_score,
    weekly_movement_score,
    focus_score,
    resonance_points,
    daily_flags,
    weekly_flags,
    now(),
    now()
  )
  on conflict (link_id) do update
    set activity_tier = excluded.activity_tier,
        sleep_tier = excluded.sleep_tier,
        recovery_tier = excluded.recovery_tier,
        hydration_tier = excluded.hydration_tier,
        consistency_score = excluded.consistency_score,
        weekly_movement_score = excluded.weekly_movement_score,
        focus_score = excluded.focus_score,
        resonance_points = excluded.resonance_points,
        daily_mission_flags = excluded.daily_mission_flags,
        weekly_mission_flags = excluded.weekly_mission_flags,
        last_refresh_at = excluded.last_refresh_at,
        updated_at = excluded.updated_at;

  delete from public.game_link_missions where link_id = link_row.id;
  insert into public.game_link_missions (link_id, mission_scope, mission_key, title, description, status, reward_key)
  values
    (
      link_row.id,
      'daily',
      'daily-steps',
      '새벽 걸음 루틴',
      '하루 평균 8,000보 이상이면 다음날 이동과 기력 보너스를 받습니다.',
      case when daily_flags ? 'daily-steps' then 'completed' else 'available' end,
      'resonance-seeds'
    ),
    (
      link_row.id,
      'daily',
      'daily-hydration',
      '정화 수분 루틴',
      '수분 점수가 높으면 회복과 작물 효율 보너스를 받습니다.',
      case when daily_flags ? 'daily-hydration' then 'completed' else 'available' end,
      'healing-broth'
    ),
    (
      link_row.id,
      'weekly',
      'weekly-consistency',
      '가장 긴 새벽의 꾸준함',
      '주간 꾸준함이 높을수록 공명 포인트와 특수 씨앗이 늘어납니다.',
      case when weekly_flags ? 'weekly-consistency' then 'completed' else 'available' end,
      'dawn-crop-pack'
    ),
    (
      link_row.id,
      'weekly',
      'weekly-movement',
      '광산 탐험 준비',
      '주간 운동량이 충분하면 채굴 효율 보너스를 받습니다.',
      case when weekly_flags ? 'weekly-movement' then 'completed' else 'available' end,
      'ore-luck-charm'
    );

  delete from public.game_link_rewards where link_id = link_row.id;
  insert into public.game_link_rewards (link_id, reward_key, reward_type, reward_payload)
  select
    link_row.id,
    mission.reward_key,
    'game_bonus',
    jsonb_build_object('sourceMission', mission.mission_key, 'status', mission.status)
  from public.game_link_missions mission
  where mission.link_id = link_row.id
    and mission.status = 'completed';

  return jsonb_build_object(
    'activityTier', activity_tier,
    'sleepTier', sleep_tier,
    'recoveryTier', recovery_tier,
    'hydrationTier', hydration_tier,
    'consistencyScore', consistency_score,
    'weeklyMovementScore', weekly_movement_score,
    'focusScore', focus_score,
    'resonancePoints', resonance_points,
    'dailyMissionFlags', coalesce(daily_flags, '[]'::jsonb),
    'weeklyMissionFlags', coalesce(weekly_flags, '[]'::jsonb),
    'lastRefreshAt', now()
  );
end;
$$;

create or replace function public.connect_game_account(
  target_profile_id uuid,
  target_user_id text,
  requested_game_account_id text
)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
  generated_token text := encode(gen_random_bytes(24), 'hex');
begin
  if not exists (
    select 1
    from public.profiles
    where id = target_profile_id
      and user_id = target_user_id
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

create or replace function public.disconnect_game_account(target_profile_id uuid)
returns void
language plpgsql
security definer
set search_path = public
as $$
begin
  update public.game_account_links
    set link_status = 'paused',
        disconnected_at = now(),
        updated_at = now()
  where profile_id = target_profile_id;
end;
$$;

create or replace function public.fetch_health_game_link_bundle(target_profile_id uuid)
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
  where profile_id = target_profile_id
  limit 1;

  if found then
    select * into profile_row
    from public.game_link_profiles
    where link_id = link_row.id
    limit 1;
  end if;

  return jsonb_build_object(
    'accountLink',
    case
      when not found and link_row.id is null then null
      else jsonb_build_object(
        'profileId', link_row.profile_id,
        'userId', link_row.user_id,
        'gameAccountId', link_row.game_account_id,
        'linkStatus', link_row.link_status,
        'linkToken', link_row.link_token,
        'linkedAt', link_row.created_at
      )
    end,
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
        )
        from public.game_link_rewards reward
        where reward.link_id = link_row.id
      ),
      '[]'::jsonb
    )
  );
end;
$$;

create or replace function public.fetch_game_link_bundle(supplied_link_token text)
returns jsonb
language plpgsql
security definer
set search_path = public
as $$
declare
  link_row public.game_account_links%rowtype;
begin
  select * into link_row
  from public.game_account_links
  where link_token = supplied_link_token
    and link_status = 'linked'
  limit 1;

  if not found then
    return jsonb_build_object(
      'accountLink', null,
      'profile', null,
      'missions', '[]'::jsonb,
      'rewards', '[]'::jsonb
    );
  end if;

  return public.fetch_health_game_link_bundle(link_row.profile_id);
end;
$$;

create or replace function public.fetch_life_sim_state(
  supplied_link_token text,
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
  requested_slot text,
  requested_state jsonb
)
returns boolean
language plpgsql
security definer
set search_path = public
as $$
declare
  link_row public.game_account_links%rowtype;
begin
  select * into link_row
  from public.game_account_links
  where link_token = supplied_link_token
    and link_status = 'linked'
  limit 1;

  if not found then
    return false;
  end if;

  insert into public.life_sim_player_states (link_id, slot, state, updated_at)
  values (link_row.id, requested_slot, requested_state, now())
  on conflict (link_id, slot) do update
    set state = excluded.state,
        updated_at = now();

  return true;
end;
$$;

grant execute on function public.connect_game_account(uuid, text, text) to anon, authenticated;
grant execute on function public.disconnect_game_account(uuid) to anon, authenticated;
grant execute on function public.refresh_game_link_profile(uuid) to anon, authenticated;
grant execute on function public.fetch_health_game_link_bundle(uuid) to anon, authenticated;
grant execute on function public.fetch_game_link_bundle(text) to anon, authenticated;
grant execute on function public.fetch_life_sim_state(text, text) to anon, authenticated;
grant execute on function public.upsert_life_sim_state(text, text, jsonb) to anon, authenticated;
