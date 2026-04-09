# DeepStakeUnity Handoff

This folder is the Unity migration handoff root.

## Purpose

The production standalone game is moving toward Unity.
The current web/Capacitor client remains a prototype/reference.

## Read first

- `../docs/deep-stake-current-systems.md`
- `../docs/deep-stake-unity-migration.md`
- `../docs/deep-stake-vertical-slice.md`
- `../docs/deep-stake-unity-tasklist.md`

## Current shared contracts

The first portable contract reference now lives in:
- `../packages/shared-types/src/deepStake.ts`
- `./contracts/README.md`

It defines Unity-ready reference types for:
- player state
- world state
- quests
- relationships
- settlement/building
- alignment/resonance/ascension
- safe boot mode and save schema

## Manual Unity Editor tasks

- use `DeepStakeUnity` as the production project root
- decide JSON vs ScriptableObject content pipeline
- create the first farm scene
- map movement input
- build local save/load path
- wire the first NPC interaction
- wire one settlement placement flow

## Repository hygiene

- keep Unity source folders under version control
- do not commit `Library`, `Temp`, `Logs`, or `UserSettings`
- if multiple nested `DeepStakeUnity` folders exist, choose one canonical root and remove the duplicate manually

No fake Unity editor output is generated here on purpose.
