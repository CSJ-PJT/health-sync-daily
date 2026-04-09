# Deep Stake Current Systems

## Current prototype status

`apps/fifth-dawn-game` is now a prototype/reference client, not the long-term production renderer.

What currently exists in the prototype:
- top-down life-sim farm loop
- local/offline-first boot path
- degraded cloud mode
- safe health-link profile consumption
- settlement/building systems converged from FitCraft
- Deep Stake alignment/resonance/ascension foundations
- mock commerce catalog foundation
- Android/Capacitor shell sufficient for prototype testing

## Systems worth porting first

These are product-level systems and should survive the Unity migration:
- player state and inventory model
- crop plots, resource nodes, hazards
- quest/story flag state
- relationship progression
- settlement/buildable object model
- Deep Stake alignment, resonance, faction affinity, ascension flags
- safe health-link contract
- save/load schema
- local/offline mode rules
- shop catalog / entitlement model

## Systems that are prototype-only

These should not be ported literally as final production architecture:
- React panel layout and mobile HUD composition
- Web canvas rendering in `renderLifeSimScene.ts`
- runtime fallback overlays and debug cards
- Capacitor-specific Android shell workarounds
- web-specific asset path fixes
- React-specific state wiring inside arena components

## Current domain entry points

Useful source folders:
- `apps/fifth-dawn-game/src/game/life-sim`
- `apps/fifth-dawn-game/src/game/settlement`
- `apps/fifth-dawn-game/src/game/shop`
- `packages/shared-types`

Renderer-coupled areas to treat carefully:
- `apps/fifth-dawn-game/src/components/*`
- `apps/fifth-dawn-game/src/App.tsx`
- `apps/fifth-dawn-game/src/components/LifeSimArena.tsx`

## Porting guidance

Port the data model and gameplay rules first.
Do not port the current React layout or Capacitor behavior as final architecture.
