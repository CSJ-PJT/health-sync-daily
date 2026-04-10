# Deep Stake Unity Editor Hookup

This is the fastest click-by-click path to get the first Deep Stake Unity slice wired in the Editor.

## Preferred path: use the menu tools

### 1. Open the project
1. Open Unity Hub.
2. Click `Add`.
3. Select `C:\Users\dan18\health-sync-daily\unity\DeepStakeUnity`.
4. Open the project.
5. Wait for script compilation to finish.

### 2. Generate the scenes automatically
1. In the Unity top menu, click `Tools`.
2. Click `Deep Stake`.
3. Click `Build Prototype Scenes`.
4. Wait for the dialog that says the three scenes were created.

This creates and wires:
- `Assets/Scenes/Boot.unity`
- `Assets/Scenes/MainMenu.unity`
- `Assets/Scenes/WorldPrototype.unity`

It also creates:
- a `DeepStakeBootstrap` object
- a `MainMenuController`
- a `WorldPrototypeController`
- a player marker with `PlayerMover2D`
- a sign with `InteractableStub`
- an Archivist stub with `QuestStubNpc`
- a settlement placement stub
- a HUD canvas with `HudStatusView`
- build settings entries for all three scenes

## What to verify after automatic scene creation

### Boot scene
1. Open `Assets/Scenes/Boot.unity`.
2. In `Hierarchy`, click `DeepStakeBootstrap`.
3. In `Inspector`, confirm:
   - `Main Menu Scene Name` = `MainMenu`
   - `World Scene Name` = `WorldPrototype`
   - `Force Local Mode` = enabled
   - `Load World Directly` = disabled

### MainMenu scene
1. Open `Assets/Scenes/MainMenu.unity`.
2. Confirm `Hierarchy` contains:
   - `MainMenuCanvas`
   - `MainMenuController`
3. Click `MainMenuController`.
4. In `Inspector`, confirm:
   - `World Scene Name` = `WorldPrototype`
   - `Headline Text` is assigned
   - `Status Text` is assigned
5. Click the `StartLocalPlay` button object.
6. In the `Button` component, confirm an `On Click()` event is present.
7. Click the `ContinueLatest` button object.
8. In the `Button` component, confirm an `On Click()` event is present.

### WorldPrototype scene
1. Open `Assets/Scenes/WorldPrototype.unity`.
2. Confirm `Hierarchy` contains:
   - `Main Camera`
   - `WorldPrototypeController`
   - `Player`
   - `FarmSign`
   - `Archivist`
   - `SettlementPlacementStub`
   - `HudCanvas`
3. Click `WorldPrototypeController`.
4. In `Inspector`, confirm:
   - `World Prototype Json` points to `Assets/Data/world-prototype.json`
   - `Player Transform` points to `Player`
   - `Npc Transform` points to `Archivist`
   - `Interactable Transform` points to `FarmSign`
   - `Primary Interactable` points to the `InteractableStub` on `FarmSign`
   - `Quest Npc` points to the `QuestStubNpc` on `Archivist`
   - `Settlement Placement` points to the `SettlementPlacementStub`
5. Click `Player`.
6. Confirm it has:
   - `SpriteRenderer`
   - `Rigidbody2D`
   - `CircleCollider2D`
   - `PlayerMover2D`
7. Click `FarmSign`.
8. Confirm it has `InteractableStub`.
9. Click `Archivist`.
10. Confirm it has `QuestStubNpc`.
11. Click `HudStatusView`.
12. Confirm `Status Text` is assigned.

## How `world-prototype.json` is connected

1. Open `Assets/Scenes/WorldPrototype.unity`.
2. Click `WorldPrototypeController`.
3. In `Inspector`, find `World Prototype Json`.
4. It should already reference `Assets/Data/world-prototype.json`.
5. If not:
   - drag `Assets/Data/world-prototype.json` from `Project` into the `World Prototype Json` field.

The controller uses the JSON to position:
- player spawn
- first NPC
- first interactable
- settlement placement origin

## How to run and verify save/load

### First run
1. Open `Assets/Scenes/Boot.unity`.
2. Click `Play`.
3. In the main menu, click `Start Local Play`.
4. In the world:
   - move with `WASD` or arrow keys
   - press `E` to trigger the sign
   - press `Q` to talk to the Archivist stub
   - press `B` to place a settlement object
   - press `F5` to save

### Verify save file path
1. While still in Play Mode, look at the Console if you add your own debug logs later.
2. Stop Play Mode.
3. Open Windows File Explorer.
4. Paste this path into the address bar:
   - `%USERPROFILE%\\AppData\\LocalLow\\Robo Heart\\Deep Stake`
5. Confirm a file named `deepstake-slot-01.json` exists.

### Verify load
1. Re-enter Play Mode from `Boot.unity`.
2. Click `Continue Latest Save`.
3. Confirm:
   - HUD status updates
   - quest completion persists
   - placed settlement object remains in the save data

## Manual path if the menu tool fails

### Create Boot scene manually
1. `File` -> `New Scene`
2. Save as `Assets/Scenes/Boot.unity`
3. Create empty object named `DeepStakeBootstrap`
4. Add component `DeepStakeBootstrap`
5. Set:
   - `Main Menu Scene Name` = `MainMenu`
   - `World Scene Name` = `WorldPrototype`
   - `Force Local Mode` = on
   - `Load World Directly` = off

### Create MainMenu scene manually
1. `File` -> `New Scene`
2. Save as `Assets/Scenes/MainMenu.unity`
3. Create `Canvas`
4. Create empty object `MainMenuController`
5. Add component `MainMenuController`
6. Create two `UI > Text` objects for headline and status
7. Assign them to `MainMenuController`
8. Create two `UI > Button` objects
9. Wire buttons to `StartLocalPlay()` and `ContinueLatest()`

### Create WorldPrototype scene manually
1. `File` -> `New Scene`
2. Save as `Assets/Scenes/WorldPrototype.unity`
3. Add `Main Camera`
4. Create empty object `WorldPrototypeController`
5. Add component `WorldPrototypeController`
6. Create `Player` object
7. Add:
   - `SpriteRenderer`
   - `Rigidbody2D`
   - `CircleCollider2D`
   - `PlayerMover2D`
8. Create `FarmSign` object and add `InteractableStub`
9. Create `Archivist` object and add `QuestStubNpc`
10. Create `SettlementPlacementStub` object and add `SettlementPlacementStub`
11. Create Canvas and `HudStatusView`
12. Assign all references in `WorldPrototypeController`
13. Drag `Assets/Data/world-prototype.json` into `World Prototype Json`

## Build settings

1. Open `File` -> `Build Settings`
2. Confirm these scenes exist in this order:
   - `Assets/Scenes/Boot.unity`
   - `Assets/Scenes/MainMenu.unity`
   - `Assets/Scenes/WorldPrototype.unity`

## Notes

- This slice is intentionally local/offline first.
- Cloud link stays optional.
- Raw health records must not be added to the Unity client.
