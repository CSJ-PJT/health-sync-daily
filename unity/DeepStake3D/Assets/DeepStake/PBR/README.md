# DeepStake PBR Pipeline

This folder is the project-side scaffold for the environment PBR pipeline.

Use it for:

- environment texture intake
- reusable material slot organization
- placeholder channels before final texture import
- decal/overlay staging

Do not place final third-party source packs directly here.
Imported source packs should stay under:

- `Assets/ThirdParty/<AssetPackName>/`

DeepStake-owned wrappers, material organization, and environment pipeline assets should live here.

Important:

- raw or curated source textures can be staged here during review
- runtime-loadable texture copies used by the current PBR pipeline must live under:
  - `Assets/Resources/PBR/Textures/Environment/`
- the current scaffold uses `placeholderColorHex * colorTint` when BaseColor is missing
- standalone roughness textures are declared in the slot library, but runtime currently uses `roughnessMultiplier` until final assets are packed into shader-compatible mask/smoothness inputs
- multi-material renderers currently receive one resolved slot across all submaterials in the scaffold path; treat that as a known limitation until final authored meshes/materials are in place
