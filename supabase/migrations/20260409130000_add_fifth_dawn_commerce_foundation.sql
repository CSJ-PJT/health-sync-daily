create table if not exists public.commerce_products (
  id text primary key,
  product_key text not null,
  title text not null,
  category text not null,
  product_type text not null,
  metadata jsonb not null default '{}'::jsonb,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create table if not exists public.commerce_purchases (
  id uuid primary key default gen_random_uuid(),
  product_id text not null references public.commerce_products(id) on delete cascade,
  product_key text not null,
  user_id uuid references auth.users(id) on delete set null,
  game_account_id text,
  platform text not null default 'mock',
  status text not null default 'pending',
  purchase_token text,
  provider_payload jsonb not null default '{}'::jsonb,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create table if not exists public.commerce_entitlements (
  id uuid primary key default gen_random_uuid(),
  product_id text not null references public.commerce_products(id) on delete cascade,
  product_key text not null,
  user_id uuid references auth.users(id) on delete cascade,
  game_account_id text,
  status text not null default 'pending',
  granted_at timestamptz,
  revoked_at timestamptz,
  source_purchase_id uuid references public.commerce_purchases(id) on delete set null,
  metadata jsonb not null default '{}'::jsonb,
  created_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create table if not exists public.fifth_dawn_unlock_mappings (
  id uuid primary key default gen_random_uuid(),
  product_id text not null references public.commerce_products(id) on delete cascade,
  unlock_key text not null,
  unlock_type text not null,
  metadata jsonb not null default '{}'::jsonb,
  created_at timestamptz not null default now()
);

create index if not exists commerce_purchases_product_key_idx on public.commerce_purchases(product_key, product_id);
create index if not exists commerce_entitlements_product_key_idx on public.commerce_entitlements(product_key, product_id);
create unique index if not exists fifth_dawn_unlock_mappings_product_unlock_idx
  on public.fifth_dawn_unlock_mappings(product_id, unlock_key);

alter table public.commerce_products enable row level security;
alter table public.commerce_purchases enable row level security;
alter table public.commerce_entitlements enable row level security;
alter table public.fifth_dawn_unlock_mappings enable row level security;

do $$
begin
  if not exists (
    select 1
    from pg_policies
    where schemaname = 'public'
      and tablename = 'commerce_products'
      and policyname = 'commerce_products_read_authenticated'
  ) then
    create policy commerce_products_read_authenticated
      on public.commerce_products
      for select
      to authenticated
      using (true);
  end if;

  if not exists (
    select 1
    from pg_policies
    where schemaname = 'public'
      and tablename = 'fifth_dawn_unlock_mappings'
      and policyname = 'fifth_dawn_unlock_mappings_read_authenticated'
  ) then
    create policy fifth_dawn_unlock_mappings_read_authenticated
      on public.fifth_dawn_unlock_mappings
      for select
      to authenticated
      using (true);
  end if;

  if not exists (
    select 1
    from pg_policies
    where schemaname = 'public'
      and tablename = 'commerce_purchases'
      and policyname = 'commerce_purchases_owner_read'
  ) then
    create policy commerce_purchases_owner_read
      on public.commerce_purchases
      for select
      to authenticated
      using (auth.uid() = user_id);
  end if;

  if not exists (
    select 1
    from pg_policies
    where schemaname = 'public'
      and tablename = 'commerce_entitlements'
      and policyname = 'commerce_entitlements_owner_read'
  ) then
    create policy commerce_entitlements_owner_read
      on public.commerce_entitlements
      for select
      to authenticated
      using (auth.uid() = user_id);
  end if;
end $$;

insert into public.commerce_products (id, product_key, title, category, product_type, metadata)
values
  ('fd_founders_pack', 'fifth-dawn', 'Founder Pack', 'founder', 'non-consumable', jsonb_build_object('fair_use', 'identity_only')),
  ('fd_starter_pack_01', 'fifth-dawn', 'Starter Pack 01', 'starter', 'non-consumable', jsonb_build_object('fair_use', 'starter_settlement')),
  ('fd_building_pack_luminous_garden', 'fifth-dawn', 'Luminous Garden Pack', 'building', 'non-consumable', jsonb_build_object('fair_use', 'garden_palette')),
  ('fd_building_pack_star_hub', 'fifth-dawn', 'Star Hub Pack', 'building', 'non-consumable', jsonb_build_object('fair_use', 'hub_palette')),
  ('fd_residence_pack_origin_home', 'fifth-dawn', 'Origin Home Residence Pack', 'residence', 'non-consumable', jsonb_build_object('fair_use', 'residence_theme')),
  ('fd_cosmetic_pack_dawn_beacons', 'fifth-dawn', 'Dawn Beacons Cosmetic Pack', 'cosmetic', 'non-consumable', jsonb_build_object('fair_use', 'cosmetics_only'))
on conflict (id) do update
set
  product_key = excluded.product_key,
  title = excluded.title,
  category = excluded.category,
  product_type = excluded.product_type,
  metadata = excluded.metadata,
  updated_at = now();

insert into public.fifth_dawn_unlock_mappings (product_id, unlock_key, unlock_type, metadata)
values
  ('fd_founders_pack', 'founder-banner', 'settlement-object', '{}'::jsonb),
  ('fd_founders_pack', 'founder-spire', 'settlement-object', '{}'::jsonb),
  ('fd_starter_pack_01', 'dawn-crate', 'settlement-object', '{}'::jsonb),
  ('fd_starter_pack_01', 'starter-lamp', 'settlement-object', '{}'::jsonb),
  ('fd_building_pack_luminous_garden', 'luminous-garden', 'settlement-theme', '{}'::jsonb),
  ('fd_building_pack_luminous_garden', 'luminous-arch', 'settlement-object', '{}'::jsonb),
  ('fd_building_pack_luminous_garden', 'garden-beacon', 'settlement-object', '{}'::jsonb),
  ('fd_building_pack_star_hub', 'star-hub-prime', 'settlement-theme', '{}'::jsonb),
  ('fd_building_pack_star_hub', 'star-gate', 'settlement-object', '{}'::jsonb),
  ('fd_building_pack_star_hub', 'portal-spire', 'settlement-object', '{}'::jsonb),
  ('fd_building_pack_star_hub', 'hub-plaza', 'settlement-object', '{}'::jsonb),
  ('fd_residence_pack_origin_home', 'origin-home', 'settlement-theme', '{}'::jsonb),
  ('fd_residence_pack_origin_home', 'origin-screen', 'settlement-object', '{}'::jsonb),
  ('fd_residence_pack_origin_home', 'origin-bed', 'settlement-object', '{}'::jsonb),
  ('fd_cosmetic_pack_dawn_beacons', 'dawn-beacon', 'beacon-skin', '{}'::jsonb)
on conflict do nothing;
