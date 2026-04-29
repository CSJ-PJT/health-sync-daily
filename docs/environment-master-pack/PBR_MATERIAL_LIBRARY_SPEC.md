# PBR Material Library Spec

This document defines the first production material library for Longest Dawn / Deep Stake environment scenes.

## Naming Convention

Use lowercase snake_case.

Format:

```text
category_surface_variant
```

Examples:

```text
ground_dirt_packed
wall_worn_concrete
roof_dark_worn_metal
prop_weathered_wood_crate
decal_rust_streak
```

## Texture Channel Naming

Each material folder should contain:

```text
<material_name>_BaseColor.png
<material_name>_Normal.png
<material_name>_Roughness.png
<material_name>_Metallic.png
<material_name>_AO.png
<material_name>_Height.png optional
<material_name>_Preview.png optional
```

## Required Ground Materials

### ground_dirt_packed
Use for main walking paths around buildings.

Mood:
- compact soil
- slightly dry
- small embedded stones
- not muddy
- readable at gameplay distance

Channels:
- BaseColor
- Normal
- Roughness
- AO
- Metallic = black / 0

### ground_dirt_dry
Use for dry outer areas and damaged village lots.

### ground_dirt_grass_mixed
Default recovery-zone outdoor ground.

Must blend:
- compact dirt
- short grass
- muted green
- subtle weeds

### ground_grass_short
Use for safe outer field and farm borders.

Must be:
- short
- muted
- not lush fantasy grass
- not too bright

### ground_farm_soil
Use for farm and greenhouse beds.

Must be:
- dark brown
- tilled rows or subtle soil ridges
- low visual noise

### ground_gravel
Use for workshop and industrial yards.

### ground_worn_asphalt
Use for city roads.

### ground_cracked_asphalt
Use for damaged checkpoint and city edges.

### ground_worn_concrete
Use for clinic entrance, sidewalks, and city pads.

### ground_sidewalk_slab
Use for city sidewalks and urban plazas.

## Required Wall Materials

### wall_worn_concrete
Default modern recovery-zone wall.

### wall_cracked_plaster
Use for clinic accents and old interiors.

### wall_red_brick_muted
Use for old city buildings.

### wall_gray_brick
Use for urban ruins and older infrastructure.

### wall_weathered_wood
Use for archive and settlement structures.

### wall_faded_painted_wood
Use for village houses and softer recovery-zone buildings.

### wall_corrugated_metal
Use for workshop, sheds, industrial buildings.

### wall_rusted_patched_metal
Use as accent panels, repairs, and patchwork walls.

### wall_reinforced_concrete
Use for checkpoint and city elite areas.

## Required Roof Materials

### roof_dark_worn_metal
Default roof material for survival buildings.

### roof_patched_sheet_metal
Use for repaired roofs and makeshift buildings.

### roof_old_tile_muted
Use for older homes and city buildings.

### roof_industrial_panel
Use for storage, workshop, warehouse.

### roof_greenhouse_frame
Use for greenhouse frame and translucent panels if supported.

## Required Prop Materials

### prop_weathered_wood_crate
Crates, small boxes, storage stacks.

### prop_rusty_barrel
Barrels, drums, oil containers.

### prop_aged_metal_sign
Signs, signposts, checkpoints.

### prop_wood_fence
Farm and village fences.

### prop_metal_fence
City and checkpoint fences.

### prop_sandbag
Barricades and defensive lines.

### prop_generator_dark_metal
Generators, machinery, power boxes.

### prop_water_tank
Water tanks and storage vessels.

### prop_notice_board
Archive boards and field notices.

## Required Decals

### decal_dirt_edge
Ground-wall contact dirt.

### decal_moss_corner
Greenish buildup in corners and wall bottoms.

### decal_rust_streak
Metal streaks, roof edges, barrels.

### decal_paint_peel
Old painted walls and metal panels.

### decal_wall_crack
Concrete and plaster cracks.

### decal_leak_stain
Vertical water stains below roofs and pipes.

### decal_tire_mark
Road and workshop yard detail.

### decal_dust
Soft settlement dust overlay.

## Material Parameter Defaults

Use these as first-pass defaults:

```text
BaseColorTint = white
NormalStrength = 0.7 to 1.0
RoughnessMultiplier = 0.8 to 1.2
AOIntensity = 0.6 to 1.0
Metallic = 0 for non-metal
Metallic = texture driven for metal
Tiling = 1.0 default, adjustable per mesh
```

## Texture Scale Guidelines

- Ground: medium tiling, avoid obvious repetition
- Walls: align to building scale, avoid huge bricks
- Roofs: slightly larger pattern, readable from camera
- Props: smaller texture scale
- Decals: world-space projection if supported

## Color Palette

Use muted survival palette:

```text
soil brown
warm gray
cold gray
faded green
desaturated rust
dark metal
weathered wood
soft warm window light
```

Avoid:

```text
pure black
pure white
neon green
bright fantasy grass
oversaturated red brick
clean plastic surfaces
```

## Acceptance Criteria

A material is acceptable if:

1. It reads clearly at gameplay camera distance.
2. It adds material realism without noise.
3. It fits A+B style.
4. It does not overpower the character.
5. It can be reused across multiple assets.
6. It works with lighting and shadows.
7. It does not require excessive unique textures per object.