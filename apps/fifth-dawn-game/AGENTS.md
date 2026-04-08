# AGENTS

## Fifth Dawn Game

- This app must remain fully playable without the health app being linked.
- Health linkage may only supply optional derived bonuses.
- Do not import or query raw health tables from here.

## Architecture

- Keep gameplay runtime under `src/game/life-sim/*`.
- Keep React shell code under `src/components/*` and `src/App.tsx`.
- Keep save/load behind repository functions so browser saves can later be swapped for filesystem saves.
- Converge reusable FitCraft building ideas into Fifth Dawn under local game modules such as settlement / housing / construction systems.
- Do not import the health app's entertainment UI directly into this app.

## Future Desktop Readiness

- Avoid browser-only assumptions in core gameplay state.
- Keep input, save, and event systems replaceable.
- Preserve fullscreen and future windowed-mode compatibility.

## Mobile Readiness

- Prefer Capacitor as the lowest-disruption first phone deployment path.
- Keep the game host fullscreen-friendly and touch-safe.
- Use app-local mobile config and scripts under this app rather than the legacy root shell.
