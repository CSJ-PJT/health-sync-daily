# Deep Stake First Quarter-View Slice

## Scene plan

- `Boot`
  - initialize local save
  - choose local/offline mode by default
- `MainMenu`
  - start local play
  - continue latest local save
- `WorldPrototype3D`
  - primitive ground plane
  - player capsule
  - Archivist cube/NPC marker
  - sign/interactable
  - settlement beacon placement point
  - quarter-view camera
  - HUD status

## Controls

- move: `WASD` / arrows
- interact: `E`
- talk to NPC: `Q`
- place settlement object: `B`
- save local slot: `F5`

## Success criteria

- scene boots in local mode
- player moves on X/Z plane
- camera follows player from quarter-view
- one sign interaction works
- one NPC quest stub works
- one settlement placement stub works
- save file is written and can be reloaded

## Temporary prototype choices allowed

- cubes, capsules, and planes
- simple UI text
- no lighting polish
- no final art

## What must already be stable

- boot flow
- local save path
- state container
- contract compatibility
- offline-first behavior
