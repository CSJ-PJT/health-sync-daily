# Deep Stake Quarter-View Migration

## Direction change

Deep Stake Unity is no longer targeting a pure 2D-first prototype as the production renderer.

The production direction is now:
- 3D quarter-view
- Project Zomboid-like feel
- perspective camera
- movement on the X/Z plane
- simple primitive world at first
- local/offline-first

## Reuse directly

- boot flow
- local save/load flow
- save schema and contracts
- quest/story/state contracts
- settlement/build placement model
- health-link safe derived-value rule
- Deep Stake lore, Longest Dawn framing, northern frontier/village/mine/farm tone

## Retire as production direction

- 2D light recovery work
- sprite/tilemap-only world assumptions
- `PlayerMover2D` as the main player controller
- 2D marker-based world scene as the main path

## Rewrite for quarter-view

- movement controller
- camera rig
- interactable placement in world space
- NPC placement in 3D space
- settlement placement visualization
- prototype world setup tools

## Recommendation

Continue inside the current `unity/DeepStake3D` project.

Reason:
- contracts, save schema, and boot scaffolding already exist
- Android/mobile settings already exist
- changing project entirely would create avoidable migration debt
- the renderer path can change inside the project without discarding current foundations

## Legacy note

- `unity/DeepStake3D` = active production path
- `unity/DeepStakeUnity` = legacy/reference only

## First production-minded 3D slice

- `Boot`
- `MainMenu`
- `WorldPrototype3D`
- quarter-view camera
- X/Z movement
- one sign interaction
- one Archivist NPC interaction
- one settlement beacon placement
- local save/load

## Constraints

- no raw health data in Unity
- no multiplayer-first focus yet
- no heavy art pipeline required for this slice
- primitive meshes are acceptable
