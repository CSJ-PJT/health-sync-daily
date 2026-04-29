# Runtime PBR Texture Intake

This folder is the runtime-loadable destination for environment PBR textures used by the current DeepStake PBR pipeline.

Expected subfolders:

- `ground/`
- `walls/`
- `roofs/`
- `props/`
- `decals/`

Place each material family in its own folder using the naming convention from:

- `docs/environment-master-pack/PBR_MATERIAL_LIBRARY_SPEC.md`

Example:

- `Assets/Resources/PBR/Textures/Environment/ground/ground_dirt_packed/ground_dirt_packed_BaseColor.png`
- `Assets/Resources/PBR/Textures/Environment/ground/ground_dirt_packed/ground_dirt_packed_Normal.png`
- `Assets/Resources/PBR/Textures/Environment/ground/ground_dirt_packed/ground_dirt_packed_Roughness.png`
- `Assets/Resources/PBR/Textures/Environment/ground/ground_dirt_packed/ground_dirt_packed_AO.png`

Third-party originals should remain under:

- `Assets/ThirdParty/<AssetPackName>/`

Only the curated, project-ready runtime copies should be placed here.
