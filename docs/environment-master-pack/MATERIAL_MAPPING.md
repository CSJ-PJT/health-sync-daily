# Material Mapping

Use this mapping for the first environment polish pass.

## Ground materials

| Material name | Use case | Notes |
|---|---|---|
| `dirt_basic` | general dirt paths | base village dirt |
| `dirt_compact` | heavily walked paths | near buildings and plaza |
| `mixed_grass_dirt` | recovery-zone center | best default outdoor material |
| `grass_basic` | outer safe zones | keep low saturation |
| `wild_grass` | edges and unused lots | use sparingly |
| `farm_soil` | farm and greenhouse beds | warmer brown |
| `cracked_dirt` | damaged village ground | not everywhere |
| `worn_asphalt` | city road | use for urban sector |
| `cracked_asphalt` | damaged roads | use near checkpoint and ruins |
| `worn_concrete` | clinic entrance, sidewalk | clean but aged |
| `sidewalk_tile` | city walkways | large readable slabs |
| `gravel_dirty` | workshop yard | good under industrial props |

## Wall materials

| Material name | Use case | Notes |
|---|---|---|
| `wall_worn_concrete` | clinic, checkpoint | primary modern wall |
| `wall_cracked_plaster` | old apartments, clinic accents | use lightly |
| `wall_red_brick` | older city buildings | muted red, not saturated |
| `wall_gray_brick` | urban ruins | neutral base |
| `wall_weathered_wood` | archive, fences | warm but muted |
| `wall_painted_wood` | settlement houses | faded paint only |
| `wall_corrugated_metal` | workshop, shed | industrial feel |
| `wall_patched_metal` | repair areas | use as accent panels |
| `wall_reinforced_concrete` | checkpoint, elite zone | heavier mood |

## Roof materials

| Material name | Use case | Notes |
|---|---|---|
| `roof_dark_metal` | clinic, workshop | reliable default roof |
| `roof_patched_sheet` | survivor buildings | visible repairs |
| `roof_old_tile` | old village/city houses | muted brown-gray |
| `roof_industrial` | workshop, storage | darker, rougher |
| `roof_greenhouse_frame` | greenhouse | transparent panels if supported |

## Prop materials

| Material name | Use case |
|---|---|
| `prop_crate_wood` | crates, boxes, storage |
| `prop_rusty_barrel` | drums and barrels |
| `prop_aged_sign_metal` | signboards, zone labels |
| `prop_fence_wood` | village/farm fences |
| `prop_fence_metal` | checkpoint/city fences |
| `prop_sandbag` | barricades and gates |
| `prop_generator_dark_metal` | generators and machinery |
| `prop_water_tank` | farm and clinic tanks |
| `prop_trash_bag` | urban debris, use sparingly |

## Overlay decals

| Overlay | Use case | Strength |
|---|---|---|
| `overlay_rust` | metal edges, barrels, gates | low-medium |
| `overlay_grime` | wall bottoms, corners | low |
| `overlay_moss` | ground-wall contact | low |
| `overlay_tire_mark` | road / workshop yard | low |
| `overlay_crack` | plaster and concrete walls | medium |
| `overlay_leak_stain` | old walls and roofs | low |
| `overlay_dust` | ground and props | low |

## Scene-specific mapping

### Recovery Field
- base ground: `mixed_grass_dirt`
- paths: `dirt_compact`
- campfire plaza: `cracked_dirt`
- clinic wall: `wall_worn_concrete`
- archive wall: `wall_weathered_wood`
- roofs: `roof_dark_metal`
- crates: `prop_crate_wood`

### Clinic Zone
- ground: `worn_concrete` near entrance, `mixed_grass_dirt` outside
- walls: `wall_worn_concrete`, `wall_cracked_plaster`
- roof: `roof_dark_metal`
- props: `prop_water_tank`, `prop_aged_sign_metal`

### Archive Zone
- ground: `dirt_compact`
- walls: `wall_weathered_wood`, `wall_gray_brick`
- roof: `roof_old_tile`
- props: `prop_crate_wood`, notice board using `prop_aged_sign_metal`

### Workshop Zone
- ground: `gravel_dirty`
- walls: `wall_corrugated_metal`, `wall_patched_metal`
- roof: `roof_industrial`
- props: `prop_generator_dark_metal`, `prop_rusty_barrel`, `prop_crate_wood`

### Farm / Greenhouse
- ground: `farm_soil`, `grass_basic`
- greenhouse: `roof_greenhouse_frame`, `wall_corrugated_metal`
- props: `prop_water_tank`, `prop_fence_wood`

### City Checkpoint
- road: `worn_asphalt`, `cracked_asphalt`
- sidewalk: `sidewalk_tile`
- walls: `wall_reinforced_concrete`, `wall_gray_brick`
- roof: `roof_dark_metal`
- props: `prop_fence_metal`, `prop_sandbag`, `prop_aged_sign_metal`
