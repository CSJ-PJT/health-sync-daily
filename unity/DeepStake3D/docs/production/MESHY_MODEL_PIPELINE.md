# Meshy Model Pipeline

## Purpose

This pipeline separates raw Meshy deliveries from curated DeepStake runtime assets.

It exists to keep:

- source ZIP intake reproducible
- runtime model copies small and game-focused
- prefab creation deferred until Unity import validation succeeds
- gameplay anchors and scene layout untouched during asset intake

## Input Location

Put new Meshy ZIP files directly under:

- `unity/DeepStake3D/Assets/ThirdParty/`

Do not put ZIP files under `Resources`.

## Source Extraction Rule

For each new ZIP:

1. Infer `assetId` from the ZIP filename and the primary model filename.
2. Extract into:
   - `unity/DeepStake3D/Assets/ThirdParty/Meshy_<assetId>/Source/`
3. Keep the full extracted source set there.

Do not treat extracted source as runtime-ready.

## Preferred Model Format

Use this selection order:

1. `GLB`
2. `FBX`

If both exist, choose `GLB` first.

If only `FBX` exists, keep using `FBX`.

## Curated Runtime Location

Copy only the selected primary model and only the textures it actually needs into:

- `unity/DeepStake3D/Assets/DeepStake/Models/Environment/<Category>/<assetId>/`

Textures go in:

- `unity/DeepStake3D/Assets/DeepStake/Models/Environment/<Category>/<assetId>/Textures/`

Do not copy:

- ZIP files
- screenshots
- preview renders
- logs
- duplicate sidecars unrelated to runtime import

## Asset Naming

Every Meshy asset should have:

- one `assetId`
- one `category`
- one registry entry in:
  - `Assets/Resources/Meshy/deepstake_meshy_model_registry.json`

If a ZIP name is ambiguous, use the model filename and intended placement role to decide the `assetId`.

## Registry Update Rule

Before placing or prefabbing a new Meshy asset:

1. Add or update its entry in:
   - `Assets/Resources/Meshy/deepstake_meshy_model_registry.json`
2. Add or update its placement intent in:
   - `Assets/Resources/Meshy/deepstake_meshy_placement_mapping.json`
3. Set status one of:
   - `pending_source`
   - `source_extracted`
   - `curated_model_ready`
   - `prefab_ready`
   - `applied`
   - `rejected`

## Prefab Creation Rule

Do not create final prefabs until Unity import validation succeeds.

Prefab creation is allowed only after:

1. Unity import completes cleanly
2. Materials are readable and non-broken
3. Scale and pivot are checked
4. Screenshot validation succeeds

Prefab target path:

- `unity/DeepStake3D/Assets/DeepStake/Prefabs/Environment/<Category>/<assetId>.prefab`

## Commit Policy

Safe to commit after validation:

- curated runtime model files
- needed curated runtime textures
- per-asset README/manifest files
- generated Unity `.meta` files for curated runtime folders/files
- registry and placement JSON updates
- production docs/checklists

Do not commit unless explicitly approved:

- raw ZIP files
- extracted ThirdParty source originals
- screenshots
- `TestResults`

## Validation Policy

Minimum gate before commit:

1. Unity import/compile validation succeeds
2. Screenshot validation succeeds
3. No gameplay logic changed
4. No scene layout changed
5. Asset passes scale/pivot/material review

If validation fails due to UPM or environment startup:

- keep source and curated folders
- do not create prefabs yet
- do not treat the asset as rejected
- leave status at `curated_model_ready`

## How To Add Future Models

For each future Meshy model:

1. Drop ZIP in `Assets/ThirdParty/`
2. Extract into `Assets/ThirdParty/Meshy_<assetId>/Source/`
3. Select `GLB` or fallback `FBX`
4. Copy only curated runtime model + required textures into `Assets/DeepStake/Models/Environment/...`
5. Add registry entry
6. Add placement mapping entry
7. Run Unity validation
8. Create prefab only after validation succeeds

## Current Blocker

Current first-batch Meshy intake is organized, but Unity validation is still intermittently blocked by:

- Unity Package Manager IPC startup failure

That is an environment blocker, not a confirmed Meshy asset pipeline failure.
