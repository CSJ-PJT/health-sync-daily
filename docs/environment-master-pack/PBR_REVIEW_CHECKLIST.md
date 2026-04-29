# PBR Review Checklist

Use this checklist after Codex completes each environment material pass.

## Screenshot requirements

Capture screenshots from the existing gameplay camera:

1. Recovery Field
2. Clinic Zone
3. Archive Zone
4. Workshop Zone
5. Farm / Greenhouse
6. City Checkpoint

Each screenshot should be compared against the current flat-color baseline.

## Phase 1 review

### Layout safety
- [ ] Existing map layout is unchanged
- [ ] Existing UI still appears
- [ ] Interaction markers still appear
- [ ] Characters still spawn correctly
- [ ] No gameplay logic was changed
- [ ] Camera angle is unchanged

### Ground pass
- [ ] Main ground is no longer flat single color
- [ ] Walkable paths are readable
- [ ] Recovery center uses mixed dirt/grass material
- [ ] Workshop yard reads as gravel/industrial ground
- [ ] City road reads as worn asphalt
- [ ] Clinic entrance reads as worn concrete
- [ ] Farm beds read as soil
- [ ] Texture repetition is not too obvious
- [ ] Ground detail does not overpower characters

### Wall pass
- [ ] Clinic wall has concrete/plaster identity
- [ ] Archive wall has wood/brick/storage identity
- [ ] Workshop wall has metal/industrial identity
- [ ] City checkpoint wall has reinforced concrete identity
- [ ] Wall materials are not all the same
- [ ] Wall detail is readable but not noisy

### Roof pass
- [ ] Roofs are darker than walls
- [ ] Roofs have distinct worn metal / patched sheet / tile identity
- [ ] Roofs improve silhouettes
- [ ] Roof detail does not dominate the scene

### Prop pass
- [ ] Crates read as weathered wood
- [ ] Barrels read as rusty metal
- [ ] Signs read as aged metal/notice surfaces
- [ ] Fences read as wood or metal depending on zone
- [ ] Props use separate material identities

### Decal pass
- [ ] Dirt edges appear near building bases
- [ ] Moss/rust/crack decals are used sparingly
- [ ] Decals increase realism without clutter
- [ ] Decals do not obscure interaction labels

### Lighting pass
- [ ] Soft directional light works
- [ ] Contact shadows appear under buildings/props/characters
- [ ] Ambient occlusion improves depth
- [ ] Color grading is subtle and warm
- [ ] No heavy fog reduces readability
- [ ] Bloom is not excessive

## Visual target checks

The scene should feel:

- [ ] less toy-like
- [ ] less placeholder-like
- [ ] more grounded
- [ ] more material-rich
- [ ] still readable
- [ ] still stylized
- [ ] not photorealistic
- [ ] not cluttered

## Fail conditions

Reject the pass if:

- the map layout changes unexpectedly
- gameplay breaks
- UI or labels become hard to read
- textures are too noisy
- everything becomes too dark
- surfaces look photorealistic and clash with characters
- the scene is improved only by adding clutter instead of materials
- ground readability is worse than before

## Review score

Score each category 1 to 5:

```text
Ground quality: _ / 5
Wall quality: _ / 5
Roof quality: _ / 5
Prop quality: _ / 5
Lighting quality: _ / 5
Character readability: _ / 5
Zone identity: _ / 5
Overall production direction: _ / 5
```

Minimum acceptable Phase 1 score:

```text
Overall production direction: 3 / 5
Character readability: 4 / 5
Ground quality: 3 / 5
```

If those are not met, repeat the material pass before adding new buildings or props.