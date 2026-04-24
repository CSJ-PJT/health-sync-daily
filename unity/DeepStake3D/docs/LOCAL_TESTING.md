# DeepStake3D Local Testing

## Fast Local Dev Mode

- Normal flow stays the same: `Boot -> MainMenu -> Start Local Play`.
- Dev autorun is available through the launch flag:
  - `-deepstakeAutorunLocalPlay`
- Mobile controls can be forced in local/editor verification with:
  - `-deepstakeForceMobileControlsInEditor`
- Optional verification tag:
  - `-deepstakeVerificationTag=local-check`

## Batch Local Tests

Run the wrapper script:

```powershell
powershell -ExecutionPolicy Bypass -File .\unity\DeepStake3D\Tools\Run-DeepStakeLocalTests.ps1
```

Artifacts are written to:

- `unity/DeepStake3D/TestResults/playmode-results.xml`
- `unity/DeepStake3D/TestResults/local-test-editor.log`

## What The PlayMode Tests Verify

- `Boot` scene loads
- `MainMenu` scene loads
- `WorldPrototype3D` scene loads
- local dev autorun reaches `WorldPrototype3D`
- player exists
- camera exists
- HUD exists
- mobile controls overlay can exist in editor test mode
- core interactables exist
- local save create/load does not crash

## Local Mobile Readability Check

Use Unity `Game View` first before a real phone build.

Suggested aspect / resolution checks:

- `720 x 1600`
- `1080 x 2400`

If installed, also use `Device Simulator`.

Inspect these items:

- UI overlap at top and bottom edges
- action button tap size
- movement pad readability
- character silhouette readability at default zoom
- camera framing around the player and nearby NPCs
- objective and prompt visibility
- whether marker labels become too small or cluttered

## Recommended Local Verification Order

1. Fix compile errors until zero.
2. Run PlayMode tests in batchmode.
3. Run Editor Play Mode with local dev autorun.
4. Check `720x1600` and `1080x2400` Game View sizes.
5. Only then do a real Android build.
