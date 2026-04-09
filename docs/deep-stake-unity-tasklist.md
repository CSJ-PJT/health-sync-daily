# Deep Stake Unity Migration Tasklist

## Immediate

These are the first tasks to unblock the production Unity client.

### Codex / repo tasks
- keep the web/Capacitor client in prototype status only
- maintain portable contracts in `packages/shared-types/src/deepStake.ts`
- normalize save payloads against `DeepStakeSaveContract`
- extract gameplay content into Unity-friendly data sources
- keep safe health-link contracts isolated from raw health tables
- document prototype-only systems vs production-worthy systems

### Unity Editor / manual tasks
- open `unity/DeepStakeUnity` in Unity Hub
- pick the canonical project root and remove accidental nested duplicate project folders if not needed
- create the first farm scene
- set up input actions for keyboard first, touch later
- add a player prefab, camera follow, and collision
- implement one NPC interaction and one quest flag flow
- implement one settlement placement flow
- implement local save/load for the first slice

## Near-Term

### Codex / repo tasks
- extract item, map, NPC, quest, and faction content into migration-friendly datasets
- define commerce entitlement payloads as renderer-agnostic contracts
- define health-link DTOs for Unity-facing safe ingestion
- add migration notes for settlement object palettes and residence themes

### Unity Editor / manual tasks
- create ScriptableObject or JSON import pipeline
- implement crop plots, resource nodes, and one hazard
- implement settlement object placement persistence
- wire one Deep Stake alignment choice and one codex/omen UI surface

## Later

### Codex / repo tasks
- evolve shared contracts as Unity needs sharpen
- maintain cloud link and entitlement contracts without pushing renderer concerns back into web prototype
- document 4D support, 5D world, and social hub domains as portable models

### Unity Editor / manual tasks
- mobile input layer
- residence themes and building packs
- 5D hub/travel gate scaffolding
- multiplayer/social hooks
- production billing adapters

## Port First

- local/offline boot
- player state and movement
- one farm map
- one NPC interaction
- one quest update
- one settlement placement
- local save/load

## Retire From Web Client

- React HUD layout as shipping UI
- Capacitor/WebView-specific fixes
- runtime fallback overlays as long-term architecture
- Android debug overlays
- web canvas rendering assumptions
