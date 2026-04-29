# First PBR 8-Slot Source Lock Checklist

Status:

- source lock drafted
- not imported yet
- pending review before any texture intake

Scope:

- `ground_dirt_grass_mixed`
- `ground_dirt_packed`
- `wall_weathered_wood`
- `wall_worn_concrete`
- `roof_dark_worn_metal`
- `prop_notice_board`
- `prop_wood_fence`
- `prop_rusty_barrel`

Rules:

- do not import until this checklist is reviewed
- do not place third-party originals under `Assets/Resources`
- originals go under `unity/DeepStake3D/Assets/ThirdParty/<AssetPackName>/`
- curated runtime copies only go under `unity/DeepStake3D/Assets/Resources/PBR/Textures/Environment/...`

Provider license references:

- AmbientCG license: <https://ambientcg.com/license>
- Poly Haven license: <https://polyhaven.com/license>

## Locked Sources

| Slot | Locked source asset page URL / identifier | Expected downloaded package/file name | License text / URL | Channels confirmed available | Preview assessment | Tileable confirmed | Intended color grading action | Final ThirdParty import path | Final Resources runtime copy path | Intended runtime output filenames |
|---|---|---|---|---|---|---|---|---|---|---|
| `ground_dirt_grass_mixed` | AmbientCG `Ground048` + AmbientCG `Grass006`  \n`https://ambientcg.com/view?id=Ground048`  \n`https://ambientcg.com/view?id=Grass006` | `Ground048_4K-PNG.zip` + `Grass006_4K-PNG.zip` | CC0 1.0  \n<https://ambientcg.com/license> | `Ground048`: BaseColor, Normal, Roughness, AO, Height expected in PBR package.  \n`Grass006`: BaseColor, Normal, Roughness, AO, Height expected in PBR package. | Acceptable as a composite. `Ground048` is somewhat realistic raw; `Grass006` must be muted heavily. | Yes, both | Blend into one muted dirt/grass sheet. Desaturate greens, reduce contrast, bias toward dusty brown-green. | `unity/DeepStake3D/Assets/ThirdParty/AmbientCG_Ground048/`  \n`unity/DeepStake3D/Assets/ThirdParty/AmbientCG_Grass006/` | `unity/DeepStake3D/Assets/Resources/PBR/Textures/Environment/ground/ground_dirt_grass_mixed/` | `ground_dirt_grass_mixed_BaseColor.png`  \n`ground_dirt_grass_mixed_Normal.png`  \n`ground_dirt_grass_mixed_Roughness.png`  \n`ground_dirt_grass_mixed_AO.png`  \noptional: `ground_dirt_grass_mixed_Height.png` |
| `ground_dirt_packed` | AmbientCG `Ground074`  \n`https://ambientcg.com/view?id=Ground074` | `Ground074_4K-PNG.zip` | CC0 1.0  \n<https://ambientcg.com/license> | BaseColor, Normal, Roughness, AO, Height expected in PBR package | Acceptable | Yes | Reduce rock contrast slightly. Keep path muted brown with readable wear, not high-detail gravel. | `unity/DeepStake3D/Assets/ThirdParty/AmbientCG_Ground074/` | `unity/DeepStake3D/Assets/Resources/PBR/Textures/Environment/ground/ground_dirt_packed/` | `ground_dirt_packed_BaseColor.png`  \n`ground_dirt_packed_Normal.png`  \n`ground_dirt_packed_Roughness.png`  \n`ground_dirt_packed_AO.png`  \noptional: `ground_dirt_packed_Height.png` |
| `wall_weathered_wood` | AmbientCG `Planks020`  \n`https://ambientcg.com/view?id=Planks020` | `Planks020_4K-PNG.zip` | CC0 1.0  \n<https://ambientcg.com/license> | BaseColor, Normal, Roughness, AO, Height expected in PBR package | Acceptable | Yes | Desaturate orange/brown warmth, deepen shadow values slightly, preserve large plank readability. | `unity/DeepStake3D/Assets/ThirdParty/AmbientCG_Planks020/` | `unity/DeepStake3D/Assets/Resources/PBR/Textures/Environment/walls/wall_weathered_wood/` | `wall_weathered_wood_BaseColor.png`  \n`wall_weathered_wood_Normal.png`  \n`wall_weathered_wood_Roughness.png`  \n`wall_weathered_wood_AO.png`  \noptional: `wall_weathered_wood_Height.png` |
| `wall_worn_concrete` | AmbientCG `Concrete024`  \n`https://ambientcg.com/view?id=Concrete024` | `Concrete024_4K-PNG.zip` | CC0 1.0  \n<https://ambientcg.com/license> | BaseColor, Normal, Roughness, AO, Height expected in PBR package | Acceptable | Yes | Shift to neutral-warm gray, suppress overly clean plaster reads, keep stains subtle. | `unity/DeepStake3D/Assets/ThirdParty/AmbientCG_Concrete024/` | `unity/DeepStake3D/Assets/Resources/PBR/Textures/Environment/walls/wall_worn_concrete/` | `wall_worn_concrete_BaseColor.png`  \n`wall_worn_concrete_Normal.png`  \n`wall_worn_concrete_Roughness.png`  \n`wall_worn_concrete_AO.png`  \noptional: `wall_worn_concrete_Height.png` |
| `roof_dark_worn_metal` | AmbientCG `CorrugatedSteel005`  \n`https://ambientcg.com/view?id=CorrugatedSteel005` | `CorrugatedSteel005_4K-PNG.zip` | CC0 1.0  \n<https://ambientcg.com/license> | BaseColor, Normal, Roughness, Metallic, AO, Height expected in PBR package | Acceptable | Yes | Darken overall value range, mute paint saturation, push toward dull charcoal-brown roof silhouette. | `unity/DeepStake3D/Assets/ThirdParty/AmbientCG_CorrugatedSteel005/` | `unity/DeepStake3D/Assets/Resources/PBR/Textures/Environment/roofs/roof_dark_worn_metal/` | `roof_dark_worn_metal_BaseColor.png`  \n`roof_dark_worn_metal_Normal.png`  \n`roof_dark_worn_metal_Roughness.png`  \n`roof_dark_worn_metal_Metallic.png`  \n`roof_dark_worn_metal_AO.png`  \noptional: `roof_dark_worn_metal_Height.png` |
| `prop_notice_board` | AmbientCG `Planks020` for the base board surface only  \n`https://ambientcg.com/view?id=Planks020` | `Planks020_4K-PNG.zip` | CC0 1.0  \n<https://ambientcg.com/license> | BaseColor, Normal, Roughness, AO, Height expected in PBR package | Acceptable for base board. Paper notice overlay is not sourced in this pass. | Yes | Grade darker than wall wood, keep base readable. Printed paper/accent overlay should be handled later as decal/overlay, not baked into this first lock. | `unity/DeepStake3D/Assets/ThirdParty/AmbientCG_Planks020/` | `unity/DeepStake3D/Assets/Resources/PBR/Textures/Environment/props/prop_notice_board/` | `prop_notice_board_BaseColor.png`  \n`prop_notice_board_Normal.png`  \n`prop_notice_board_Roughness.png`  \n`prop_notice_board_AO.png`  \noptional: `prop_notice_board_Height.png` |
| `prop_wood_fence` | AmbientCG `Fence001`  \n`https://ambientcg.com/view?id=Fence001` | `Fence001_4K-PNG.zip` | CC0 1.0  \n<https://ambientcg.com/license> | BaseColor, Normal, Roughness, AO, Height expected in PBR package | Acceptable | Yes | Dry out the wood, lower contrast slightly, keep fence darker than open ground but lighter than roof mass. | `unity/DeepStake3D/Assets/ThirdParty/AmbientCG_Fence001/` | `unity/DeepStake3D/Assets/Resources/PBR/Textures/Environment/props/prop_wood_fence/` | `prop_wood_fence_BaseColor.png`  \n`prop_wood_fence_Normal.png`  \n`prop_wood_fence_Roughness.png`  \n`prop_wood_fence_AO.png`  \noptional: `prop_wood_fence_Height.png` |
| `prop_rusty_barrel` | AmbientCG `Metal015`  \n`https://ambientcg.com/view?id=Metal015` | `Metal015_4K-PNG.zip` | CC0 1.0  \n<https://ambientcg.com/license> | BaseColor, Normal, Roughness, Metallic, AO, Height expected in PBR package | Acceptable | Yes | Desaturate rust, compress highlight range, keep metal dark and rough rather than orange and glossy. | `unity/DeepStake3D/Assets/ThirdParty/AmbientCG_Metal015/` | `unity/DeepStake3D/Assets/Resources/PBR/Textures/Environment/props/prop_rusty_barrel/` | `prop_rusty_barrel_BaseColor.png`  \n`prop_rusty_barrel_Normal.png`  \n`prop_rusty_barrel_Roughness.png`  \n`prop_rusty_barrel_Metallic.png`  \n`prop_rusty_barrel_AO.png`  \noptional: `prop_rusty_barrel_Height.png` |

## Notes

- `ground_dirt_grass_mixed` is intentionally locked as a curated composite, not a single downloaded material.
- `prop_notice_board` is locked only for the wood board base in this pass. Paper notices / printed accents should be handled later in the decal or overlay pass.
- All eight locks assume `4K-PNG` source packages for the first intake pass. Do not mix JPG and PNG variants inside the same slot family.
- Standalone roughness and AO maps are acceptable for the current runtime scaffold. No channel repack is required before first import review.

## Stop Condition

Do not import any texture files until this source lock checklist is reviewed and accepted.
