alter table if exists public.game_account_links
  add column if not exists product_key text not null default 'fifth-dawn';

alter table if exists public.game_link_profiles
  add column if not exists product_key text not null default 'fifth-dawn';

alter table if exists public.game_link_missions
  add column if not exists product_key text not null default 'fifth-dawn';

alter table if exists public.game_link_rewards
  add column if not exists product_key text not null default 'fifth-dawn';

alter table if exists public.life_sim_player_states
  add column if not exists product_key text not null default 'fifth-dawn';

create index if not exists game_account_links_product_key_idx
  on public.game_account_links (product_key);

create index if not exists game_link_profiles_product_key_idx
  on public.game_link_profiles (product_key);

create index if not exists game_link_missions_product_key_idx
  on public.game_link_missions (product_key);

create index if not exists game_link_rewards_product_key_idx
  on public.game_link_rewards (product_key);

create index if not exists life_sim_player_states_product_key_idx
  on public.life_sim_player_states (product_key);

create unique index if not exists game_account_links_profile_product_uidx
  on public.game_account_links (profile_id, product_key);

create unique index if not exists game_link_profiles_link_product_uidx
  on public.game_link_profiles (profile_id, product_key);
