create table if not exists public.entertainment_life_sim_saves (
  id text primary key,
  profile_id uuid references public.profiles(id) on delete cascade not null,
  user_id text,
  slot text not null default 'main',
  state jsonb not null default '{}'::jsonb,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now(),
  unique (profile_id, slot)
);

create index if not exists entertainment_life_sim_saves_profile_slot_idx
  on public.entertainment_life_sim_saves(profile_id, slot);

alter table public.entertainment_life_sim_saves enable row level security;

drop policy if exists "Life sim saves baseline select" on public.entertainment_life_sim_saves;
drop policy if exists "Life sim saves baseline insert" on public.entertainment_life_sim_saves;
drop policy if exists "Life sim saves baseline update" on public.entertainment_life_sim_saves;
drop policy if exists "Life sim saves baseline delete" on public.entertainment_life_sim_saves;

create policy "Life sim saves baseline select"
  on public.entertainment_life_sim_saves for select
  using (true);

create policy "Life sim saves baseline insert"
  on public.entertainment_life_sim_saves for insert
  with check (true);

create policy "Life sim saves baseline update"
  on public.entertainment_life_sim_saves for update
  using (true)
  with check (true);

create policy "Life sim saves baseline delete"
  on public.entertainment_life_sim_saves for delete
  using (true);
