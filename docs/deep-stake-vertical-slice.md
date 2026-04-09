# Deep Stake Unity First Vertical Slice

## Goal

Produce a Unity slice that proves the production game loop without depending on the prototype renderer.

## Required slice

### Boot
- boot directly into local/offline mode
- no cloud dependency required
- optional cloud-ready state notice only

### Playable map
- one farm/home map
- top-down movement
- collision
- one interactable sign or bed

### One NPC and one quest
- Archivist or Mechanic
- one dialogue interaction
- one quest flag update

### One settlement action
- place one buildable object
- save that placement
- reload and confirm persistence

### Save/load
- local save slot
- restore player position, inventory, farm plots, quest flags, and settlement state

## Not required for the first Unity slice

- full mobile UI parity
- commerce UI
- full Deep Stake codex
- full 5D world hub
- final touch/controller support

## Success criteria

- app boots locally
- player moves on map
- one interaction works
- one quest progresses
- one settlement object can be placed
- save/load restores the result
