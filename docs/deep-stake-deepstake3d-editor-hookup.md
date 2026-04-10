# DeepStake3D Editor Hookup

`DeepStake3D` is the only active Unity production client.

## Fastest path

1. Open `C:\Users\dan18\health-sync-daily\unity\DeepStake3D` in Unity Hub.
2. Wait for import and script compilation.
3. Open the top menu `Tools > Deep Stake 3D`.
4. Click `Build Quarter-View Prototype Scenes`.
5. Open `Assets/Scenes/Boot.unity`.
6. Press `Play`.
7. In `MainMenu`, click `Start Local Play`.
8. In `WorldPrototype3D`:
   - move with `WASD`
   - interact with `E`
   - talk with `Q`
   - place beacon with `B`
   - save with `F5`

## Save path

Windows local save should appear under:

`%USERPROFILE%\AppData\LocalLow\Robo Heart\Deep Stake`

Expected file:

`deepstake-slot-01.json`

## Legacy note

Do not continue implementation inside `DeepStakeUnity`.
Use it only as a reference source for migrated logic.
