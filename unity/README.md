# Deep Stake Unity Handoff

This folder is the Unity migration handoff root.

## Purpose

`DeepStake3D` is the active Unity production client.
`DeepStakeUnity` is legacy/reference only.
The current web/Capacitor client remains a prototype/reference.

## Read first

- `../docs/deep-stake-current-systems.md`
- `../docs/deep-stake-unity-migration.md`
- `../docs/deep-stake-vertical-slice.md`
- `../docs/deep-stake-first-unity-slice.md`
- `../docs/deep-stake-quarter-view-migration.md`
- `../docs/deep-stake-first-quarter-view-slice.md`
- `../docs/deep-stake-factions.md`
- `../docs/deep-stake-world-baseline.md`
- `../docs/deep-stake-deepstake3d-editor-hookup.md`
- `../docs/deep-stake-health-link-contract.md`
- `../docs/deep-stake-unity-tasklist.md`

## Current shared contracts

The first portable contract reference now lives in:
- `../packages/shared-types/src/deepStake.ts`
- `./contracts/README.md`
- `./DeepStake3D/Assets/Scripts/README.md`

It defines Unity-ready reference types for:
- player state
- world state
- quests
- relationships
- settlement/building
- alignment/resonance/ascension
- safe boot mode and save schema

## Manual Unity Editor tasks

- use `DeepStake3D` as the only active project root
- import the ported 3D scaffold and let Unity generate/update `.meta` files
- run `Tools > Deep Stake 3D > Build Quarter-View Prototype Scenes`
- verify `Boot`, `MainMenu`, and `WorldPrototype3D`
- validate local save/load and quarter-view controller flow
- treat `DeepStakeUnity` as behavior reference only

## Repository hygiene

- keep Unity source folders under version control
- do not commit `Library`, `Temp`, `Logs`, or `UserSettings`
- `DeepStake3D` is canonical
- do not continue active implementation in `DeepStakeUnity`

No fake Unity editor output is generated here on purpose.
