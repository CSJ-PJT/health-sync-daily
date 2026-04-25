# Local Automation Workflow

## What This Does

`Run-DeepStakeAutoLoop.ps1` is the top-level local screenshot command for `DeepStake3D`.

It wraps the lower-level screenshot script and does this in one run:

- runs `PreflightOnly` first
- checks for blocking Unity processes
- optionally kills blocking Unity processes
- runs the requested capture mode
- verifies that the expected PNG exists
- writes a simple text report
- maintains a `latest-local.png` alias

## Default Command

```powershell
powershell -ExecutionPolicy Bypass -File .\unity\DeepStake3D\Tools\Run-DeepStakeAutoLoop.ps1 -VerificationTag "auto-test"
```

Default mode is `EditorRender`.

## Force-Kill Mode

If Unity-related background processes are blocking capture:

```powershell
powershell -ExecutionPolicy Bypass -File .\unity\DeepStake3D\Tools\Run-DeepStakeAutoLoop.ps1 -VerificationTag "auto-test" -ForceKillBlockingProcesses
```

This can kill:

- `Unity`
- `Unity Hub`
- `Unity.Licensing.Client`
- `UnityPackageManager`
- `UnityShaderCompiler`

It retries preflight once after killing blockers. It does not loop forever.

## Fallback Modes

`EditorPlay`:

```powershell
powershell -ExecutionPolicy Bypass -File .\unity\DeepStake3D\Tools\Run-DeepStakeAutoLoop.ps1 -Mode EditorPlay -VerificationTag "auto-test"
```

`WindowsPlayer`:

```powershell
powershell -ExecutionPolicy Bypass -File .\unity\DeepStake3D\Tools\Run-DeepStakeAutoLoop.ps1 -Mode WindowsPlayer -VerificationTag "auto-test"
```

`PreflightOnly`:

```powershell
powershell -ExecutionPolicy Bypass -File .\unity\DeepStake3D\Tools\Run-DeepStakeAutoLoop.ps1 -Mode PreflightOnly
```

## Useful Switches

- `-VerificationTag <tag>`
- `-Mode <EditorRender|EditorPlay|WindowsPlayer|PreflightOnly>`
- `-ForceKillBlockingProcesses`
- `-TimeoutSeconds <n>`
- `-KeepPreviousLatest`
- `-NoLatestAlias`

## Expected Output Files

Screenshot:

- `unity/DeepStake3D/Pictures/Screenshot/local-<tag>.png`

Latest alias:

- `unity/DeepStake3D/Pictures/Screenshot/latest-local.png`

Previous latest alias:

- `unity/DeepStake3D/Pictures/Screenshot/latest-local.previous.png`

Status file:

- `unity/DeepStake3D/TestResults/local-windows-screenshot-status.txt`

Run report:

- `unity/DeepStake3D/TestResults/local-auto-loop-report.txt`

Logs:

- `unity/DeepStake3D/TestResults/local-windows-build.log`
- `unity/DeepStake3D/TestResults/local-windows-runtime.log`

## How To Verify

Successful run:

- expected `local-<tag>.png` exists
- `latest-local.png` updates unless `-NoLatestAlias` is used
- report file says `success=true`

Blocked run:

- status file contains `state=blocked`
- report file contains the failure reason

## What Not To Touch

- do not manually edit screenshot artifacts inside `Pictures/Screenshot`
- do not commit `TestResults`, `.utmp`, or generated PNG files
- do not use this workflow for `DeepStakeUnity`
