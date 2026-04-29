# Codex Texture Instruction

Read this file before editing any environment scene.

## Main instruction
Do not redesign the map. Keep the current camera, layout, gameplay UI, interactions, and logic.

Upgrade the current scene using texture, material, lighting, and readability improvements only.

## Style target
A+B style:

- MapleStory2-like readability and clear forms
- Rust / State of Decay-like grounded survival mood
- muted colors
- stylized 3D, not photorealistic
- readable from quarter/isometric camera
- clean but worn materials
- controlled dirt and weathering

## First-pass implementation
1. Find all current scene meshes and material assignments.
2. Replace flat-color materials with named reusable materials.
3. Add texture slots for all required material categories.
4. Apply material mapping from `MATERIAL_MAPPING.md`.
5. Improve lighting and shadows.
6. Add contact shadows or ambient occlusion if available.
7. Keep characters and interaction markers readable.
8. Summarize changed files.

## Material rules

### Ground
Use different materials for different surfaces.

- grass areas: `grass_basic` or `mixed_grass_dirt`
- village paths: `dirt_basic`, `dirt_compact`, or `village_path`
- city roads: `worn_asphalt` or `cracked_asphalt`
- clinic entrance: `worn_concrete`
- greenhouse/farm: `farm_soil`, `greenhouse_floor`

### Walls
Do not use one material for every building.

- clinic: worn concrete / faded plaster
- archive: weathered wood + worn concrete
- workshop: corrugated metal + patched metal
- greenhouse: glass frame + metal frame + dirt floor
- checkpoint: reinforced concrete + dark metal
- city elite zone: dark concrete + polished but worn metal

### Roofs
Roofs should have darker and simpler material than walls.

- metal roof
- patched roof sheet
- old roof tile
- industrial roof

### Props
Props should have separate material identities.

- crates: weathered wood
- barrels: rusty metal
- signs: aged metal
- fences: wood or metal depending on zone
- generators: dark metal with rust overlay
- water tanks: dull blue-gray metal or plastic

## Lighting
- use soft directional lighting
- add real shadows if possible
- add ambient occlusion or contact shadow
- avoid heavy fog
- avoid cinematic bloom
- keep UI and characters visible

## Do not
- Do not add many new props to hide weak materials.
- Do not make the map dirtier just to make it realistic.
- Do not use hyper-real PBR textures that clash with stylized characters.
- Do not change gameplay behavior.
- Do not remove existing UI or interaction markers.
