# First PBR 8-Slot Acquisition Plan

This document covers the first real texture acquisition/import pass for these eight runtime slots only:

1. `ground_dirt_grass_mixed`
2. `ground_dirt_packed`
3. `wall_weathered_wood`
4. `wall_worn_concrete`
5. `roof_dark_worn_metal`
6. `prop_notice_board`
7. `prop_wood_fence`
8. `prop_rusty_barrel`

This is an intake plan only.

Rules:

- do not import large random packs
- do not place third-party source originals under `Assets/Resources`
- do not change gameplay logic
- do not change scene layout
- do not add placeholder art just to fill slots

## Source Policy

Third-party source originals:

- `unity/DeepStake3D/Assets/ThirdParty/<AssetPackName>/`

Curated runtime texture copies only:

- `unity/DeepStake3D/Assets/Resources/PBR/Textures/Environment/<family>/<slot>/`

## Asset Source Status

Actual source texture files for these eight slots are not available in the repository yet.

Because the source files are missing, this pass does not lock final file names from a real asset pack yet.
Instead, it defines:

- preferred source category
- preferred license/source family
- required channel list
- grading/repacking expectations
- exact runtime destination mapping

## Slot Intake Table

| Slot | Source asset candidate | License / source location | Required channels | Color grading needed | Channel repacking needed | Runtime destination |
|---|---|---|---|---|---|---|
| `ground_dirt_grass_mixed` | Pending acquisition. Preferred candidate: curated CC0 tileable stylized dirt-grass blend from `Poly Haven` or `AmbientCG`, or a custom blend authored from two CC0 bases. | `Assets/ThirdParty/<AssetPackName>/ground/ground_dirt_grass_mixed/` from a CC0 source pack. | BaseColor, Normal, Roughness, AO. Height optional. Metallic not required. | Yes. Must mute greens and suppress fantasy saturation. | No strict repack required for metallic. Roughness stays standalone in source; runtime currently uses multiplier fallback. | `Assets/Resources/PBR/Textures/Environment/ground/ground_dirt_grass_mixed/` |
| `ground_dirt_packed` | Pending acquisition. Preferred candidate: compact path dirt with small stones from `Poly Haven` or `AmbientCG`. | `Assets/ThirdParty/<AssetPackName>/ground/ground_dirt_packed/` from a CC0 source pack. | BaseColor, Normal, Roughness, AO. Height optional. Metallic not required. | Yes. Keep muted brown and reduce high-contrast pebbles if needed. | No strict repack required. Roughness remains standalone in source. | `Assets/Resources/PBR/Textures/Environment/ground/ground_dirt_packed/` |
| `wall_weathered_wood` | Pending acquisition. Preferred candidate: weathered plank wall material from `Poly Haven` or `AmbientCG`, or custom graded painted-plank base. | `Assets/ThirdParty/<AssetPackName>/walls/wall_weathered_wood/` from a CC0 source pack. | BaseColor, Normal, Roughness, AO. Height optional. Metallic not required. | Yes. Desaturate warm browns and keep board contrast readable at quarter-view scale. | No strict repack required. Roughness remains standalone in source. | `Assets/Resources/PBR/Textures/Environment/walls/wall_weathered_wood/` |
| `wall_worn_concrete` | Pending acquisition. Preferred candidate: aged concrete wall from `Poly Haven` or `AmbientCG`. | `Assets/ThirdParty/<AssetPackName>/walls/wall_worn_concrete/` from a CC0 source pack. | BaseColor, Normal, Roughness, AO. Height optional. Metallic not required. | Yes. Keep concrete neutral-warm and avoid clinical pure gray/white. | No strict repack required. Roughness remains standalone in source. | `Assets/Resources/PBR/Textures/Environment/walls/wall_worn_concrete/` |
| `roof_dark_worn_metal` | Pending acquisition. Preferred candidate: worn sheet-metal roof from `Poly Haven`, `AmbientCG`, or custom graded industrial roofing set. | `Assets/ThirdParty/<AssetPackName>/roofs/roof_dark_worn_metal/` from a CC0 source pack. | BaseColor, Normal, Roughness, Metallic, AO. Height optional. | Yes. Darken and mute to roof silhouette range; remove bright specular reads. | Metallic should remain separate if source provides it. Roughness remains standalone in source. | `Assets/Resources/PBR/Textures/Environment/roofs/roof_dark_worn_metal/` |
| `prop_notice_board` | Pending acquisition. Preferred candidate: weathered wood signboard or bulletin-board material set, potentially assembled from wood + paper/sign sources. | `Assets/ThirdParty/<AssetPackName>/props/prop_notice_board/` from a CC0 source pack or custom assembled source set. | BaseColor, Normal, Roughness, AO. Height optional. Metallic not required unless a metal frame variant is added later. | Yes. Keep paper/sign accents readable without becoming bright UI-like noise. | No strict repack required. Roughness remains standalone in source. | `Assets/Resources/PBR/Textures/Environment/props/prop_notice_board/` |
| `prop_wood_fence` | Pending acquisition. Preferred candidate: simple weathered fence wood material from `Poly Haven` or `AmbientCG`. | `Assets/ThirdParty/<AssetPackName>/props/prop_wood_fence/` from a CC0 source pack. | BaseColor, Normal, Roughness, AO. Height optional. Metallic not required. | Yes. Keep it dry, muted, and slightly darker than open ground. | No strict repack required. Roughness remains standalone in source. | `Assets/Resources/PBR/Textures/Environment/props/prop_wood_fence/` |
| `prop_rusty_barrel` | Pending acquisition. Preferred candidate: rusty industrial barrel material from `Poly Haven` or `AmbientCG`. | `Assets/ThirdParty/<AssetPackName>/props/prop_rusty_barrel/` from a CC0 source pack. | BaseColor, Normal, Roughness, Metallic, AO. Height optional. | Yes. Desaturate orange rust and keep metal dark enough for the scene palette. | Metallic should remain separate if source provides it. Roughness remains standalone in source. | `Assets/Resources/PBR/Textures/Environment/props/prop_rusty_barrel/` |

## Required Runtime File Mapping

These are the expected runtime file names once the source textures are curated and copied into `Resources`.

### ground_dirt_grass_mixed

- `ground_dirt_grass_mixed_BaseColor.png`
- `ground_dirt_grass_mixed_Normal.png`
- `ground_dirt_grass_mixed_Roughness.png`
- `ground_dirt_grass_mixed_AO.png`
- optional: `ground_dirt_grass_mixed_Height.png`

### ground_dirt_packed

- `ground_dirt_packed_BaseColor.png`
- `ground_dirt_packed_Normal.png`
- `ground_dirt_packed_Roughness.png`
- `ground_dirt_packed_AO.png`
- optional: `ground_dirt_packed_Height.png`

### wall_weathered_wood

- `wall_weathered_wood_BaseColor.png`
- `wall_weathered_wood_Normal.png`
- `wall_weathered_wood_Roughness.png`
- `wall_weathered_wood_AO.png`
- optional: `wall_weathered_wood_Height.png`

### wall_worn_concrete

- `wall_worn_concrete_BaseColor.png`
- `wall_worn_concrete_Normal.png`
- `wall_worn_concrete_Roughness.png`
- `wall_worn_concrete_AO.png`
- optional: `wall_worn_concrete_Height.png`

### roof_dark_worn_metal

- `roof_dark_worn_metal_BaseColor.png`
- `roof_dark_worn_metal_Normal.png`
- `roof_dark_worn_metal_Roughness.png`
- `roof_dark_worn_metal_Metallic.png`
- `roof_dark_worn_metal_AO.png`
- optional: `roof_dark_worn_metal_Height.png`

### prop_notice_board

- `prop_notice_board_BaseColor.png`
- `prop_notice_board_Normal.png`
- `prop_notice_board_Roughness.png`
- `prop_notice_board_AO.png`
- optional: `prop_notice_board_Height.png`

### prop_wood_fence

- `prop_wood_fence_BaseColor.png`
- `prop_wood_fence_Normal.png`
- `prop_wood_fence_Roughness.png`
- `prop_wood_fence_AO.png`
- optional: `prop_wood_fence_Height.png`

### prop_rusty_barrel

- `prop_rusty_barrel_BaseColor.png`
- `prop_rusty_barrel_Normal.png`
- `prop_rusty_barrel_Roughness.png`
- `prop_rusty_barrel_Metallic.png`
- `prop_rusty_barrel_AO.png`
- optional: `prop_rusty_barrel_Height.png`

## Intake Checklist Before Import

For each slot:

- confirm the source is tileable where the slot is a tiling environment material
- confirm the source includes at least BaseColor, Normal, Roughness, AO
- reject the source if it is too photorealistic, too saturated, too noisy, or too plastic
- record the source pack name and license inside `Assets/ThirdParty/<AssetPackName>/`
- copy only the curated runtime textures into `Assets/Resources/PBR/Textures/Environment/...`
- do not copy demo scenes, unused models, or full packs into `Resources`

## Required User/Importer Action Before Scene Work

Before any actual scene-facing PBR pass:

1. choose the specific free source pack or CC0 source set
2. import originals under `Assets/ThirdParty/<AssetPackName>/`
3. curate and grade only the eight runtime materials above
4. copy only the required runtime channel textures into `Resources`
5. validate one screenshot pass before expanding beyond these eight slots

## Stop Rule

Do not continue to broad environment replacement until at least these eight slots exist as real curated runtime textures and pass screenshot review.
