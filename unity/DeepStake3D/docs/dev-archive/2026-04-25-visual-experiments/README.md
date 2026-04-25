# 2026-04-25 Visual Experiments Archive

Baseline context:
- Active project: `unity/DeepStake3D`
- Baseline pushed commit before this archive: `948ab0e`
- Archived file: `WorldPrototypeVisualPass-experiment.patch`

Why this diff was archived:
- `WorldPrototypeVisualPass.cs` accumulated too many mixed visual experiments in one file.
- The file now combines useful composition reset ideas with weak facade-detail experiments and earlier ground-art generator evolution.
- The latest screenshots showed partial improvements, but not a strong enough overall jump to justify committing the entire mixed diff.
- Archiving the diff preserves the experiment without forcing the next pass to inherit a noisy baseline.

Useful ideas to preserve:
- Lower background blockers instead of oversized rounded background blobs.
- Reduced strange rounded background shapes in the top/right backdrop.
- Reduced circular/ring ground look in the first screen.
- Notice board silhouette improvements.
- Player readability preservation while composition changes were tested.

Weak ideas to avoid:
- Tiny facade details that do not show in the screenshot.
- Broad terrain/carpet expansion.
- Repeated tint-only building changes.

Next clean direction:
- Write a short target document first.
- Pick one focused pass at a time.
- Start again from a clean `WorldPrototypeVisualPass.cs` baseline.
- Reapply only the strongest archived ideas deliberately, not as another accumulated experiment stack.
