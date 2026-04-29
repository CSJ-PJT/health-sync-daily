# PBR Source Asset List

This is the ordered production source-asset list for the Longest Dawn / Deep Stake environment upgrade.

The goal is not to collect random realistic textures. The goal is to collect or create stylized PBR materials that match the A+B style target:

- A: MapleStory2-like readability
- B: Rust / State of Decay-like grounded survival material realism
- Final: stylized PBR, muted, readable, believable, not photorealistic

## Source rules

Use assets only if they support these channels where possible:

```text
BaseColor / Albedo
Normal
Roughness
Metallic
Ambient Occlusion
Height optional
```

Avoid assets that are:

- too photorealistic
- too noisy at gameplay camera distance
- too clean / plastic
- too saturated
- too fantasy-like
- too high contrast
- visually incompatible with stylized characters

## Phase 1 — Must-have materials

These are required before the first serious visual pass.

### 1. ground_dirt_grass_mixed
Use:
- recovery field center
- safe-zone outdoor ground
- village edges

Search / creation keywords:

```text
stylized PBR dirt grass ground, muted survival settlement, short grass, compact soil, readable tileable material
```

Acceptance:
- muted brown + muted green
- no fantasy lush grass
- no tiny noisy detail
- readable from isometric camera

### 2. ground_dirt_packed
Use:
- main walking paths
- around buildings
- camp center

Keywords:

```text
packed dirt path PBR, compact soil, small stones, worn walking path, muted brown, tileable
```

### 3. ground_worn_asphalt
Use:
- city roads
- checkpoint road

Keywords:

```text
worn asphalt PBR, cracked road, faded urban road, muted gray, tileable, survival city
```

### 4. ground_worn_concrete
Use:
- clinic entrance
- sidewalk pads
- building foundations

Keywords:

```text
worn concrete slab PBR, aged concrete floor, subtle cracks, muted gray, tileable
```

### 5. wall_worn_concrete
Use:
- clinic
- checkpoint
- modern buildings

Keywords:

```text
worn concrete wall PBR, aged modern concrete, subtle stains, muted gray, tileable
```

### 6. wall_weathered_wood
Use:
- archive
- settlement buildings
- fences

Keywords:

```text
weathered wood wall PBR, faded planks, survival settlement, muted brown, tileable
```

### 7. wall_corrugated_metal
Use:
- workshop
- shed
- storage

Keywords:

```text
corrugated metal PBR, weathered sheet metal, muted rust, industrial shed, tileable
```

### 8. roof_dark_worn_metal
Use:
- default survival roofs
- clinic roof
- workshop roof

Keywords:

```text
dark worn metal roof PBR, patched roof sheet, muted gray brown, survival settlement roof
```

### 9. prop_weathered_wood_crate
Use:
- crates
- storage boxes
- supply stacks

Keywords:

```text
weathered wooden crate PBR material, old wood, survival supply box, muted brown
```

### 10. prop_rusty_barrel
Use:
- barrels
- workshop props
- city debris

Keywords:

```text
rusty metal barrel PBR, worn industrial drum, muted rust, rough metal
```

## Phase 2 — Important materials

### 11. ground_gravel
Use:
- workshop yard
- industrial area

### 12. ground_farm_soil
Use:
- farm
- greenhouse beds

### 13. ground_sidewalk_slab
Use:
- city sidewalk
- plaza

### 14. wall_gray_brick
Use:
- urban ruins
- old city walls

### 15. wall_cracked_plaster
Use:
- clinic accents
- damaged interiors

### 16. wall_rusted_patched_metal
Use:
- workshop repair panels
- makeshift settlement walls

### 17. roof_patched_sheet_metal
Use:
- improvised buildings
- survivor shelters

### 18. prop_aged_metal_sign
Use:
- signs
- checkpoint markers
- field notices

### 19. prop_metal_fence
Use:
- checkpoint
- city perimeter

### 20. prop_sandbag
Use:
- barricades
- checkpoint defense

## Phase 3 — Decals / overlays

These should be used to increase realism without adding clutter.

### decal_dirt_edge
Use at:
- wall bottoms
- building base
- road edges

### decal_moss_corner
Use at:
- concrete corners
- old walls
- shaded ground

### decal_rust_streak
Use at:
- metal roof edges
- barrels
- workshop panels

### decal_paint_peel
Use at:
- painted wood
- old signage
- metal doors

### decal_wall_crack
Use at:
- concrete walls
- plaster walls

### decal_leak_stain
Use at:
- below roofs
- below pipes
- old concrete walls

### decal_tire_mark
Use at:
- asphalt road
- workshop yard

### decal_dust
Use at:
- ground blending
- prop contact points

## Suggested source options

Use one or more of these sources:

1. Quixel Megascans — high-quality PBR base, must be stylized through color grading and scale control
2. Poly Haven — good CC0 PBR materials, must be filtered for style match
3. AmbientCG — useful CC0 PBR materials, needs curation
4. Substance / Designer / Painter — best for custom final look
5. Blender procedural materials — good for placeholders and generated variants
6. Meshy / AI-generated material references — only for concept and placeholder; not final unless quality is verified

## Curation checklist

Before importing a material, check:

- Is it tileable?
- Does it include Normal and Roughness?
- Is the color muted enough?
- Is the detail readable from gameplay camera?
- Does it fit stylized characters?
- Can it be reused in multiple places?
- Does it avoid obvious repetition?
- Does it work under warm daylight and ambient occlusion?

## First acquisition batch

Collect or create these first:

```text
ground_dirt_grass_mixed
ground_dirt_packed
ground_worn_asphalt
ground_worn_concrete
wall_worn_concrete
wall_weathered_wood
wall_corrugated_metal
roof_dark_worn_metal
prop_weathered_wood_crate
prop_rusty_barrel
```

Do not proceed to large building replacement until this first material batch is working in the current scene.