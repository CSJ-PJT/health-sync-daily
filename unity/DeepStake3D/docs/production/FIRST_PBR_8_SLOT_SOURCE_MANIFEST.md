# First PBR 8-Slot Source Manifest

Status:

- first controlled intake completed
- runtime copies optimized for commit-size reduction
- pending screenshot review before any commit

License:

- AmbientCG CC0 1.0
- license URL: <https://ambientcg.com/license>

Rules:

- third-party originals remain under `unity/DeepStake3D/Assets/ThirdParty/`
- curated runtime textures only remain under `unity/DeepStake3D/Assets/Resources/PBR/Textures/Environment/`
- no final commit should include 4K ZIP packages or source sidecars unless Git LFS is explicitly approved

## Slot Manifest

| Slot | Source IDs | Source URLs | ThirdParty source folder | Runtime folder | Grading applied | Runtime target resolution | Runtime channels kept |
|---|---|---|---|---|---|---|---|
| `ground_dirt_grass_mixed` | `Ground048`, `Grass006` | <https://ambientcg.com/view?id=Ground048>  /  <https://ambientcg.com/view?id=Grass006> | `Assets/ThirdParty/AmbientCG_Ground048/` and `Assets/ThirdParty/AmbientCG_Grass006/` | `Assets/Resources/PBR/Textures/Environment/ground/ground_dirt_grass_mixed/` | Composite blend. Ground base kept dusty-brown, grass overlay desaturated and reduced to a low-contrast brown-green mix. | `2048` | `BaseColor`, `Normal`, `Roughness`, `AO` |
| `ground_dirt_packed` | `Ground074` | <https://ambientcg.com/view?id=Ground074> | `Assets/ThirdParty/AmbientCG_Ground074/` | `Assets/Resources/PBR/Textures/Environment/ground/ground_dirt_packed/` | Muted brown path, lowered stone contrast, preserved readable packed-soil wear. | `2048` | `BaseColor`, `Normal`, `Roughness`, `AO` |
| `wall_weathered_wood` | `Planks020` | <https://ambientcg.com/view?id=Planks020> | `Assets/ThirdParty/AmbientCG_Planks020/` | `Assets/Resources/PBR/Textures/Environment/walls/wall_weathered_wood/` | Desaturated warm wood, deeper shadow values, preserved broad plank readability. | `1024` | `BaseColor`, `Normal`, `Roughness`, `AO` |
| `wall_worn_concrete` | `Concrete024` | <https://ambientcg.com/view?id=Concrete024> | `Assets/ThirdParty/AmbientCG_Concrete024/` | `Assets/Resources/PBR/Textures/Environment/walls/wall_worn_concrete/` | Shifted toward neutral-warm gray and reduced sterile clean-concrete feel. | `1024` | `BaseColor`, `Normal`, `Roughness` |
| `roof_dark_worn_metal` | `CorrugatedSteel005` | <https://ambientcg.com/view?id=CorrugatedSteel005> | `Assets/ThirdParty/AmbientCG_CorrugatedSteel005/` | `Assets/Resources/PBR/Textures/Environment/roofs/roof_dark_worn_metal/` | Darkened overall value range, muted surface color, kept silhouette in dull charcoal-brown range. | `2048` | `BaseColor`, `Normal`, `Roughness`, `Metallic`, `AO` |
| `prop_notice_board` | `Planks020` | <https://ambientcg.com/view?id=Planks020> | `Assets/ThirdParty/AmbientCG_Planks020/` | `Assets/Resources/PBR/Textures/Environment/props/prop_notice_board/` | Darker board wood than wall wood. Paper notice overlay intentionally deferred to later decal/overlay pass. | `1024` | `BaseColor`, `Normal`, `Roughness`, `AO` |
| `prop_wood_fence` | `Fence001` | <https://ambientcg.com/view?id=Fence001> | `Assets/ThirdParty/AmbientCG_Fence001/` | `Assets/Resources/PBR/Textures/Environment/props/prop_wood_fence/` | Dried and muted fence wood, lowered contrast, kept lighter than roof mass. | `1024` | `BaseColor`, `Normal`, `Roughness` |
| `prop_rusty_barrel` | `Metal015` | <https://ambientcg.com/view?id=Metal015> | `Assets/ThirdParty/AmbientCG_Metal015/` | `Assets/Resources/PBR/Textures/Environment/props/prop_rusty_barrel/` | Desaturated rust and compressed highlights to keep the barrel dark and rough instead of bright orange. | `1024` | `BaseColor`, `Normal`, `Roughness`, `Metallic` |

## Optimization Decisions

- default runtime resolution reduced to `1024` for walls and props
- `2048` kept only for:
  - `ground_dirt_grass_mixed`
  - `ground_dirt_packed`
  - `roof_dark_worn_metal`
- `Height` maps were removed from the curated runtime copies in this pass
  - reason: optional only, not needed for first scene validation, and expensive in repo/storage cost

## Missing Runtime Channels

- `wall_worn_concrete`: no source `AO`
- `prop_wood_fence`: no source `AO`
- `prop_rusty_barrel`: no source `AO`

These missing channels did not block the validation pass.
