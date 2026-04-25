This archive stores the incompatible first-screen ground-art editor menu that was left half-present in the workspace.

Why it was archived:
- The clean baseline `WorldPrototypeVisualPass.cs` no longer contains the temporary ground-art generator entry points.
- The untracked editor menu still referenced:
  - `WorldPrototypeVisualPass.RefreshFirstScreenGroundArt(...)`
  - `WorldPrototypeVisualPass.ClearFirstScreenGroundArt(...)`
- That mismatch caused Unity editor compilation to fail before screenshot validation.

Archived files:
- `DeepStakeFirstScreenGroundArtMenu.cs.txt`
- `DeepStakeFirstScreenGroundArtMenu.cs.meta.txt`

Current intent:
- Keep the tooling idea for later recovery.
- Remove it from `Assets/` so Unity does not compile it in the current clean baseline.
- Do not reintroduce these references unless the ground-art generator is restored intentionally as a tracked feature.
