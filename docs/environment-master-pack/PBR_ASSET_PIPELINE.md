# PBR Asset Pipeline

This is the production pipeline for upgrading the current environment from flat low-poly placeholder visuals into a polished stylized PBR survival settlement.

## Goal

Create an environment pipeline that supports:

- production-quality PBR materials
- reusable material definitions
- consistent texture naming
- controlled stylized realism
- readable quarter/isometric gameplay camera
- scalable asset import from external tools such as Meshy, Blender, Substance, Quixel, Poly Haven, or custom generated textures

## Style Direction

Final style: A+B

A: MapleStory2-like readability
- clear silhouettes
- readable from distance
- clean shape language
- not visually noisy

B: Rust / State of Decay-like survival material realism
- muted color palette
- believable wear
- damaged modern settlement surfaces
- practical urban recovery-zone mood

Final blend:
- stylized PBR
- grounded but not photorealistic
- detailed but not noisy
- realistic materials on readable low/mid-poly forms

## Pipeline Stages

### Stage 0 — Current State
Current environment is mostly flat-colored low-poly geometry.

Problems:
- weak material separation
- flat ground
- simple roofs and walls
- low surface depth
- repeated colors
- limited lighting depth

Do not solve this by adding clutter.

### Stage 1 — Material Slot Setup
Before importing final textures, create reusable material slots.

Required categories:

```text
Ground
Walls
Roofs
Props
Overlays / Decals
Lighting Profiles
```

Each material should expose:

```text
BaseColor / Albedo
Normal
Roughness
Metallic
AmbientOcclusion
Height optional
Tiling / UV Scale
Color Tint
Roughness Multiplier
Normal Strength
AO Strength
```

### Stage 2 — Production Texture Import
Import production textures using the required naming convention.

Each PBR material folder should look like:

```text
material_name/
  material_name_BaseColor.png
  material_name_Normal.png
  material_name_Roughness.png
  material_name_Metallic.png
  material_name_AO.png
  material_name_Height.png optional
  material_name_Preview.png optional
```

Example:

```text
ground_dirt_packed/
  ground_dirt_packed_BaseColor.png
  ground_dirt_packed_Normal.png
  ground_dirt_packed_Roughness.png
  ground_dirt_packed_Metallic.png
  ground_dirt_packed_AO.png
```

### Stage 3 — Material Mapping
Apply materials according to `MATERIAL_MAPPING.md` and `PRODUCTION_PBR_QUALITY_TARGET.md`.

Priority:

1. Ground
2. Walls
3. Roofs
4. Props
5. Decals / overlays
6. Lighting

### Stage 4 — Decal Pass
Use decals to create realism without clutter.

Recommended decal types:

```text
decal_dirt_edge
decal_moss_corner
decal_rust_streak
decal_paint_peel
decal_wall_crack
decal_leak_stain
decal_tire_mark
decal_dust
```

Recommended placement:

- bottom of walls
- roof edges
- ground/wall contact points
- city road corners
- workshop machinery area
- checkpoint barriers
- near drainage / pipes

### Stage 5 — Lighting Pass
Lighting must support material quality and readability.

Required lighting changes:

- soft directional light
- contact shadows
- ambient occlusion
- subtle warm grade
- no heavy fog
- low bloom
- readable UI and labels

### Stage 6 — Quality Review
Review the scene using these checks:

- Can the player identify walkable ground?
- Can the player identify buildings by function?
- Are characters readable above the background?
- Are interaction labels visible?
- Are materials varied but consistent?
- Are textures too noisy at gameplay zoom?
- Are props supporting function, not clutter?

## Folder Structure

Recommended project structure:

```text
assets/
  textures/
    environment/
      ground/
      walls/
      roofs/
      props/
      decals/
  materials/
    environment/
      ground/
      walls/
      roofs/
      props/
      decals/
  models/
    environment/
      buildings/
      props/
      modular/
  scenes/
    environment/
  docs/
    environment-master-pack/
```

If the project already has a different asset path, preserve the existing convention and add a clear environment subfolder.

## Texture Resolution Policy

Use:

- 1024px for normal environment surfaces
- 2048px for hero buildings or large repeated surfaces
- 512px only for small props or mobile fallback

Avoid 4K unless absolutely necessary.

## Performance Policy

- Use mipmaps.
- Use texture compression appropriate to the engine.
- Reuse material instances.
- Avoid unique materials for every object.
- Use decals sparingly.
- Keep draw calls under control.
- Use atlases only where they do not reduce material quality.

## Non-Goals

Do not:

- redesign the whole map during this pass
- replace all models before material polish
- add clutter to hide weak materials
- make everything dirty
- use fully photorealistic textures that clash with stylized characters
- make surfaces too noisy for mobile readability

## Production Rule

The scene should feel like:

"A readable stylized survival settlement with believable PBR materials."

Not:

"A flat low-poly prototype" or "a photorealistic realistic survival simulator."