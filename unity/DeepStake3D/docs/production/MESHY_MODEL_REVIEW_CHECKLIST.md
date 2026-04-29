# Meshy Model Review Checklist

Use this checklist before approving any Meshy environment asset for commit or placement.

## Import Readiness

- source ZIP is archived under `Assets/ThirdParty/`
- curated runtime model is under `Assets/DeepStake/Models/Environment/...`
- asset has a registry entry
- asset has a placement mapping entry

## Scale Check

- asset scale is plausible against nearby DeepStake gameplay anchors
- doors read as door-sized
- windows read as window-sized
- fences read as modular border segments
- props do not dominate building masses

## Pivot Check

- pivot is usable for placement
- floor props sit correctly on ground
- attachment pieces align to wall/entry faces
- modular pieces have practical snap/alignment pivots

## Material Check

- no missing materials
- no broken pink materials
- no obviously inverted normal response
- roughness/metallic response is believable from the quarter-view camera
- no lime green
- no neon
- no over-bright emissive unless intentionally justified

## Texture Check

- only needed textures are included in runtime folders
- no preview or screenshot files were copied into runtime folders
- texture resolution is reasonable for runtime use
- readable surfaces do not blur excessively at game camera scale

## Camera Readability

- silhouette reads at the current quarter/isometric camera
- no tiny detail dependence
- landmark assets remain recognizable at first glance
- boundary assets reduce empty test-map feeling instead of adding noise

## Placement Safety

- no scene layout change was introduced
- no gameplay anchor moved
- no gameplay logic changed
- no interaction marker behavior changed

## Validation Gate

- Unity import/compile validation succeeded
- screenshot validation succeeded
- asset-specific issues were documented if any remain

## Commit Gate

- raw ZIP not included
- ThirdParty extracted source not included unless explicitly approved
- screenshots not included
- `TestResults` not included
- curated runtime files only
