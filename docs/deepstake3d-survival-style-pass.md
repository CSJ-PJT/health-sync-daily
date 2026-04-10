# DeepStake3D Survival Style Pass

This pass is meant to push the current prototype toward a harsher quarter-view survival mood without rewriting the core loop.

## What it changes

The Unity editor menu `Tools > Deep Stake 3D > Apply Survival Style Pass` updates the existing prototype scenes in a non-destructive way:

- Main menu background, button palette, and text colors
- World camera background color and narrower field of view
- Directional light tone for a colder dusty atmosphere
- Generated materials for ground, road, archive surfaces, markers, and proxy actors
- World scene material references on `WorldPrototype3DController`
- `PlayerMover3D` movement tuning for a heavier, slower feel
- HUD placement and top-left status panel treatment

## Why this is safer

This approach avoids hand-editing scene YAML in GitHub and avoids changing the game state flow.
The style pass works as an editor tool so you can run it locally, inspect the result in Unity, and keep or revert the scene changes before committing them.

## Current tuning direction

### Palette
- background: deep blue-gray / soot tone
- field: muted olive
- roads: dark asphalt gray
- archive walls: weathered concrete
- props: rusted brown
- UI text: warm paper-gray instead of pure white

### Motion
- lower top speed
- softer acceleration
- slower turn speed
- slightly stronger gravity

The goal is not to copy any existing game directly, but to move DeepStake3D closer to a bleak survival readability profile.

## Recommended workflow

1. Open `unity/DeepStake3D` in Unity.
2. Let Unity import the new editor script and generate `.meta` files locally if needed.
3. Run `Tools > Deep Stake 3D > Apply Survival Style Pass`.
4. Review `MainMenu` and `WorldPrototype3D`.
5. Tweak values in the script if you want a stronger or softer look.
6. Commit the resulting scene/material changes only after visual review.

## Next safe steps

After this pass, the safest next additions would be:

- a dedicated tile/blockout material set for buildings and interiors
- a proper icon-based HUD skin
- animation blend tuning for stop/start weight
- a separate `DesignSandbox3D` scene for testing mood and props without touching the live prototype
