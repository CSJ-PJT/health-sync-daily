# Monorepo Architecture

## Product Split

The repository is split into two products:

1. `apps/health-app`
2. `apps/fifth-dawn-game`

The health app owns raw health ingestion and derivation.
The game app owns gameplay and consumes only derived parameters.

## Product Boundary

- `apps/health-app` remains the health product.
- `apps/fifth-dawn-game` is the standalone Fifth Dawn product.
- `Pulse Frontier` remains inside the health app entertainment hub for now.
- `FitCraft Island` is preserved as a source of building-system ideas and data structures, but it is converging into Fifth Dawn rather than becoming its own standalone app.

## Shared Packages

- `@health-sync/shared-types`
  - shared contracts for game-link data
- `@health-sync/shared-auth`
  - lightweight shared storage helpers for current identity model
- `@health-sync/shared-profile`
  - shared profile summary helpers
- `@health-sync/game-link-sdk`
  - client-side RPC wrapper for linkage flows
- `@health-sync/design-system`
  - placeholder shared UI package for later extraction

## Linkage Contract

The game app receives only derived values such as:

- `activityTier`
- `sleepTier`
- `recoveryTier`
- `hydrationTier`
- `consistencyScore`
- `weeklyMovementScore`
- `focusScore`
- `resonancePoints`
- `dailyMissionFlags`
- `weeklyMissionFlags`
- `lastRefreshAt`

No raw heart-rate streams, full sleep records, nutrition logs, or body composition rows are exposed to the game app.

## Persistence

### Health App

- existing Supabase tables
- existing normalized health data and domain tables

### Game App

- local save is the baseline
- linked cloud save is optional
- `life_sim_player_states` stores linked standalone save slots

## Security Model

- New linkage and life-sim tables use RLS.
- Access is primarily through RPC functions:
  - `connect_game_account`
  - `disconnect_game_account`
  - `refresh_game_link_profile`
  - `fetch_health_game_link_bundle`
  - `fetch_game_link_bundle`
  - `fetch_life_sim_state`
  - `upsert_life_sim_state`

## Current Compromise

The app still relies on the repository's existing local `profile_id` / `user_id` identity model rather than a fully tightened authenticated user session. Because of that:

- linkage RPCs are safer than open table access
- health-side RPCs now verify `profile_id + user_id` together before returning link data
- but they still need a stricter auth model later

Next hardening step:

- bind linkage RPCs and table policies to first-class authenticated ownership rather than local profile identifiers alone

## Fifth Dawn Convergence Note

The current Fifth Dawn app now reuses the creative-building direction from FitCraft Island through an in-game settlement/residence builder foundation.
This avoids maintaining two separate long-term building products while preserving the useful work already done.
