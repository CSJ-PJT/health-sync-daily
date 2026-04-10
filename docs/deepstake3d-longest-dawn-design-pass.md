# DeepStake3D Longest Dawn Design Pass

This pass extends the earlier survival-style work and leans more directly into the active story bible.

## Story direction used

The pass follows the current DeepStake3D codex guidance:

- first zone is `Longest Dawn`
- the first slice should communicate recovery, records, settlement, and structural pressure
- `Continuum Directorate` should be felt through traces such as repeated notices, delayed supplies, and missing records
- the world should still read as salvageable rather than purely ruined

## What this pass adds

Run this menu inside Unity:

`Tools > Deep Stake 3D > Apply Longest Dawn Design Pass`

It adds:

- Main menu story tags that frame the first slice as Longest Dawn
- extra world props such as ledger crates, supply pallets, fences, barriers, and an observer frame
- pressure markers like route audit notices and incomplete archive copies
- extra HUD lore tags for zone mood and pressure traces
- a cuter player proxy direction by shrinking the body slightly, adding a larger round head, and adding a simple bag accent

## Character direction

The request was to avoid a harsher survival-avatar read and make the current placeholder feel cuter.
This pass does that without introducing a full character pipeline.

Current proxy choices:

- slightly smaller capsule body
- larger round head silhouette
- softer muted outfit color
- small backpack accent

This keeps the game grounded, but makes the playable proxy friendlier and easier to read.

## Why this is still safe

This remains an editor-driven pass instead of hand-editing scene YAML in GitHub.
It avoids rewriting the bootstrap, game state, save flow, or interaction loop.

## Recommended order in Unity

1. Open `unity/DeepStake3D`.
2. Run `Tools > Deep Stake 3D > Apply Survival Style Pass` first.
3. Run `Tools > Deep Stake 3D > Apply Longest Dawn Design Pass` second.
4. Review `MainMenu` and `WorldPrototype3D`.
5. Commit generated scene/material changes after visual review.

## Best next visual upgrades

- replace proxy player with a simple chibi low-poly mesh
- add road edge decals and field row variation
- add archive outpost window/door silhouettes
- introduce icon-backed HUD widgets instead of text-only hints
- add a warm beacon glow so recovery objects stand out from pressure objects
