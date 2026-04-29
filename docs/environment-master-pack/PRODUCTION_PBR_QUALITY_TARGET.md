# Production PBR Quality Target

This document supersedes the previous prototype texture-pack direction.

## Important
The first generated ZIP texture pack is only a prototype / material-slot test pack.
It is not the final quality target.

The target quality is a production-style PBR material pass similar to the reference guide provided by the user:

- Albedo / BaseColor
- Normal
- Roughness
- Metallic where needed
- Ambient Occlusion
- optional Height / Displacement where supported
- overlay decals such as dirt, moss, rust, paint peel, tire marks, cracks, leaks

## Visual target
The scene should look like a polished stylized 3D survival settlement.

Style target: A+B

A:
- readable shapes like MapleStory2
- clear silhouettes
- low visual clutter
- mobile readable

B:
- grounded survival mood like Rust / State of Decay
- muted colors
- believable worn surfaces
- functional buildings and props

Final mix:
- not photorealistic
- not flat low-poly placeholder
- not bright cartoon
- not noisy realism
- stylized PBR with controlled wear

## Required material channels
Each production material should support these channels where the engine allows:

```text
BaseColor / Albedo
Normal
Roughness
Metallic
AmbientOcclusion
Optional Height
```

## Texture resolution target
Use this priority:

- Prototype: 512px
- First production pass: 1024px
- Important hero surfaces: 2048px

Avoid 4K unless absolutely necessary.

## Required material families

### Ground
- ground_dirt_packed
- ground_dirt_dry
- ground_dirt_grass_mixed
- ground_grass_short
- ground_farm_soil
- ground_gravel
- ground_worn_asphalt
- ground_cracked_asphalt
- ground_worn_concrete
- ground_sidewalk_slab

### Walls
- wall_worn_concrete
- wall_cracked_plaster
- wall_red_brick_muted
- wall_gray_brick
- wall_weathered_wood
- wall_faded_painted_wood
- wall_corrugated_metal
- wall_rusted_patched_metal
- wall_reinforced_concrete

### Roofs
- roof_dark_worn_metal
- roof_patched_sheet_metal
- roof_old_tile_muted
- roof_industrial_panel
- roof_greenhouse_frame

### Props
- prop_weathered_wood_crate
- prop_rusty_barrel
- prop_aged_metal_sign
- prop_wood_fence
- prop_metal_fence
- prop_sandbag
- prop_generator_dark_metal
- prop_water_tank
- prop_notice_board

### Overlays / decals
- decal_dirt_edge
- decal_moss_corner
- decal_rust_streak
- decal_paint_peel
- decal_wall_crack
- decal_leak_stain
- decal_tire_mark
- decal_dust

## Scene application rules

### Ground pass
The ground should create most of the improvement.
Replace plain base planes with blended terrain materials.

- recovery center: ground_dirt_grass_mixed
- walking paths: ground_dirt_packed
- workshop yard: ground_gravel
- city roads: ground_worn_asphalt and ground_cracked_asphalt
- clinic entrance: ground_worn_concrete
- farm: ground_farm_soil and ground_grass_short

### Wall pass
Buildings must not share one flat material.
Each building needs separate wall, roof, door, window, and trim materials.

### Roof pass
Roofs should be darker, simpler, and less noisy than walls.
They should provide strong silhouette readability.

### Decal pass
Use decals to create realism without clutter.
Do not manually add too many props to hide poor materials.

Recommended decal locations:
- wall bottoms
- ground-wall contact points
- metal roof edges
- road corners
- near workshop machinery
- checkpoint barricades

## Lighting target
Use lighting to support material quality:

- soft directional light
- contact shadows
- ambient occlusion
- subtle warm color grading
- no heavy fog
- no excessive bloom
- readable UI and interactable labels

## Codex task
When implementing the next environment pass, Codex should:

1. Preserve existing map layout and gameplay.
2. Replace flat materials with production PBR material definitions.
3. Create material slots for BaseColor, Normal, Roughness, Metallic, AO.
4. Add support for overlay decals if the engine supports it.
5. Use texture tiling controls per material.
6. Add material scale controls to avoid obvious repetition.
7. Improve lighting, shadows, AO, and color grading.
8. Keep character readability higher than background detail.

## Do not
- Do not treat the prototype ZIP as final quality.
- Do not use noisy high-detail textures everywhere.
- Do not redesign the entire map in this pass.
- Do not add clutter before the material pass is complete.
- Do not let the scene become photorealistic and clash with stylized characters.
