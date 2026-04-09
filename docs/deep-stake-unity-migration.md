# Deep Stake Unity Migration

## Product direction

The health app stays on the current stack.
The standalone game moves toward Unity as the production client.
The current web/Capacitor Fifth Dawn client remains as:
- prototype
- behavior reference
- migration source
- temporary harness

## What Unity should port first

### Core domain
- save contract
- player movement state
- inventory/resources
- farm plots and crop progression
- resource nodes and hazards
- quests and story flags
- NPC relationship state
- settlement/buildable data
- Deep Stake alignment/resonance model
- safe health-link bundle contract

### Core behavior
- local/offline boot rules
- degraded cloud behavior
- one farm map
- one village interaction
- one mine interaction
- one settlement placement flow

## What Unity should not copy literally

- React component tree
- web-only HUD composition
- Capacitor mobile shell
- Android WebView fixes
- runtime fallback overlays
- canvas renderer internals from the prototype

## Recommended Unity architecture

- Unity project: `DeepStakeUnity`
- data-driven gameplay layer
- JSON/ScriptableObject content for maps, items, quests, factions, shop products
- save/load using the shared save contract as reference
- safe cloud link adapter separated from core gameplay
- renderer-independent domain services where possible

## Suggested migration order

1. import shared save/domain contract reference
2. build local/offline boot
3. main map + movement
4. one NPC interaction and one quest
5. one settlement placement flow
6. local save/load
7. derived health-link consumption
8. expand story, settlement, and commerce systems

## Health-link rule

Unity must only consume derived, game-safe values:
- activity tier
- sleep tier
- recovery tier
- hydration tier
- consistency score
- weekly movement score
- focus score
- resonance points
- daily/weekly mission flags

Unity must not consume raw health records directly.
