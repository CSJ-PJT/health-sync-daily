# Fifth Dawn Valley Vertical Slice

## Overview

`Fifth Dawn Valley` is now a standalone app under `apps/fifth-dawn-game`.

It is a top-down life-sim / farming RPG vertical slice with:

- farm map
- village square map
- mine / ruins map
- crop loop
- energy and day cycle
- NPC dialogue
- resource nodes and a hazard
- local save
- optional linked cloud save

## Current Slice

### Maps

- `Recovery Farm`
- `Dawn Square`
- `Purifier Ruins`

### Farming Loop

- hoe soil
- plant dawn turnip seeds
- water crop
- sleep to advance day
- harvest crop

### NPCs

- Archivist Aria
- Mechanic Doyun

Each has at least one conditional dialogue branch.

### Health Link Bonus Scaffold

The game can optionally read derived linkage values and convert them into small bonuses:

- activity -> starting energy
- sleep/recovery -> next-day recovery
- consistency/hydration -> crop efficiency

These bonuses are optional. The game remains fully playable without linkage.

## Runtime Structure

- `src/game/life-sim/*` holds game state, data, and render logic
- `src/components/LifeSimArena.tsx` hosts the playable slice
- `src/components/FullscreenGameHost.tsx` manages fullscreen-capable presentation
- `src/services/repositories/lifeSimSaveRepository.ts` handles local + optional linked save persistence

## Future Work

- proper camera smoothing and scale settings
- file-based desktop saves
- richer NPC schedules
- crafting and placement systems
- story flags toward the Longest Dawn and Fifth Earth arc
- future achievements bridge for desktop / Steam
