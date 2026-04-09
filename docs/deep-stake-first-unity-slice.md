# Deep Stake First Unity Slice

## Slice objective

Build a serious production foundation in Unity without waiting for final art or online systems.

## Required scene plan

- `Boot`
  - decide local/offline vs cloud-capable mode
  - initialize save service
  - load prototype data
- `MainMenu`
  - start local play
  - continue latest local save
  - show cloud link status as optional only
- `WorldPrototype`
  - top-down or isometric-friendly map
  - player movement
  - one interaction
  - one NPC
  - one quest
  - one settlement placement
  - simple HUD

## Required gameplay loop

1. Boot in local mode.
2. Spawn player on farm map.
3. Move around with keyboard.
4. Interact with one sign or NPC.
5. Complete one quest flag.
6. Place one settlement object.
7. Save and reload the result.

## What can stay temporary in the first slice

- placeholder tiles
- primitive sprites or markers
- minimal UI styling
- debug-friendly HUD
- one map only

## What must already be production-minded

- local save schema
- renderer-independent data contracts
- optional cloud link boundary
- settlement object model
- quest/story flag model
- alignment/resonance compatibility in save state
