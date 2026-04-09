# Deep Stake Unity Scripts

This folder is the first production-side script foundation for Deep Stake.

## Current bootstrap layout

- `Boot/`
  - boot mode selection
  - prototype scene startup
- `Contracts/`
  - portable save/domain models for Unity-side use
- `Core/`
  - runtime state container
- `HealthLink/`
  - safe derived health-link model only
- `Interaction/`
  - interactable stub contracts
- `Player/`
  - simple movement/controller logic
- `Quests/`
  - first quest/NPC stub logic
- `Save/`
  - local JSON save service
- `Settlement/`
  - first placement stub
- `UI/`
  - debug HUD / boot status
- `World/`
  - prototype world bootstrap

## Important rule

Do not make these scripts depend on raw health records.
Cloud features stay optional and must degrade cleanly to local mode.
