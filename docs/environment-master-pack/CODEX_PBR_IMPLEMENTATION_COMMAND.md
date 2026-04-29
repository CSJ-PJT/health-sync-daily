# Codex PBR Implementation Command

Copy this command into Codex when starting the production PBR environment pass.

```text
Read these files first:

- docs/environment-master-pack/PBR_ASSET_PIPELINE.md
- docs/environment-master-pack/PBR_MATERIAL_LIBRARY_SPEC.md
- docs/environment-master-pack/PRODUCTION_PBR_QUALITY_TARGET.md
- docs/environment-master-pack/MATERIAL_MAPPING.md
- docs/environment-master-pack/ASSET_PLACEMENT_GUIDE.md

The goal is to build a high-quality PBR environment asset pipeline for the current game scene.

Important:
The previous generated texture ZIP is only a prototype material-slot test pack. Do not treat it as final quality.

Task:
Implement the code-side and project-structure side of the PBR material pipeline.

Keep:
- existing map layout
- existing gameplay logic
- existing UI
- existing camera angle
- existing interaction markers

Do not redesign the map.
Do not add clutter to fake quality.

Implement:

1. Create or organize asset folders for environment PBR textures:
   - ground
   - walls
   - roofs
   - props
   - decals / overlays

2. Add reusable PBR material definitions or material config entries for:
   - BaseColor / Albedo
   - Normal
   - Roughness
   - Metallic
   - Ambient Occlusion
   - optional Height
   - UV tiling / texture scale
   - color tint
   - normal strength
   - roughness multiplier
   - AO intensity

3. Create the first production material slots listed in `PBR_MATERIAL_LIBRARY_SPEC.md`.

4. Apply material mapping to the current scene according to `MATERIAL_MAPPING.md`:
   - recovery field: mixed dirt/grass, compact dirt paths
   - clinic: worn concrete and cracked plaster
   - archive: weathered wood and gray brick
   - workshop: gravel ground, corrugated metal, patched metal
   - farm: farm soil and short grass
   - city checkpoint: worn asphalt, cracked asphalt, reinforced concrete, metal fence

5. Add support for overlay/decal materials if the engine supports it:
   - dirt edge
   - moss corner
   - rust streak
   - paint peel
   - wall crack
   - leak stain
   - tire mark
   - dust

6. Add or improve lighting settings:
   - soft directional light
   - contact shadows
   - ambient occlusion
   - subtle warm color grading
   - avoid heavy fog
   - avoid strong bloom

7. Add placeholder material slots where final textures are missing.
   The placeholder should make missing assets obvious in code but should not break the scene.

8. Add documentation comments or config comments showing where final PBR textures should be placed.

9. Run the project checks available in this repository.

10. Summarize:
   - files changed
   - material slots created
   - scene mappings applied
   - missing final PBR texture assets
   - next recommended art tasks

Style target:
A+B
- MapleStory2-like readability
- Rust / State of Decay-like grounded survival material realism
- muted colors
- stylized PBR
- readable quarter/isometric camera
- not photorealistic
- not flat prototype
```
