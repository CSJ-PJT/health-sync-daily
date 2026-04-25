param(
    [string]$UnityPath,
    [switch]$SkipBuild,
    [string]$VerificationTag = "local-dev-run",
    [Alias("LaunchTimeoutSeconds")]
    [int]$TimeoutSeconds = 0,
    [switch]$FailIfUnityProcessesRunning = $true,
    [ValidateSet("EditorRender", "EditorPlay", "WindowsPlayer")]
    [string]$Mode = "EditorRender"
)

$ErrorActionPreference = "Stop"

$projectPath = Split-Path -Parent $PSScriptRoot
$windowsBuildRoot = Join-Path $projectPath "Builds\Windows"
$verificationRoot = Join-Path $projectPath "TestResults"
$picturesRoot = Join-Path $projectPath "Pictures\Screenshot"
$statusPath = Join-Path $verificationRoot "local-windows-screenshot-status.txt"
$defaultEditorLogPath = Join-Path $verificationRoot "local-windows-build.log"
$defaultRuntimeLogPath = Join-Path $verificationRoot "local-windows-runtime.log"
$exePath = Join-Path $windowsBuildRoot "DeepStake-local-dev.exe"
$statusNotes = New-Object System.Collections.Generic.List[string]

New-Item -ItemType Directory -Force -Path $windowsBuildRoot | Out-Null
New-Item -ItemType Directory -Force -Path $verificationRoot | Out-Null
New-Item -ItemType Directory -Force -Path $picturesRoot | Out-Null

function Resolve-UnityEditorPath {
    param([string]$PreferredPath)

    if ($PreferredPath -and (Test-Path $PreferredPath)) {
        return (Resolve-Path $PreferredPath).Path
    }

    if ($env:UNITY_EDITOR_PATH -and (Test-Path $env:UNITY_EDITOR_PATH)) {
        return (Resolve-Path $env:UNITY_EDITOR_PATH).Path
    }

    $candidates = @(Get-ChildItem "C:\Program Files\Unity\Hub\Editor" -Directory -ErrorAction SilentlyContinue |
        Sort-Object Name -Descending |
        ForEach-Object { Join-Path $_.FullName "Editor\Unity.exe" } |
        Where-Object { Test-Path $_ })

    if ($candidates.Count -gt 0) {
        return $candidates[0]
    }

    throw "Unity.exe not found. Pass -UnityPath or set UNITY_EDITOR_PATH."
}

function Remove-IfExists {
    param([string]$Path)

    if (Test-Path $Path) {
        try {
            Remove-Item -LiteralPath $Path -Force -ErrorAction Stop
            return $true
        }
        catch {
            return $false
        }
    }

    return $true
}

function Add-StatusNote {
    param([string]$Note)

    if (-not [string]::IsNullOrWhiteSpace($Note)) {
        $script:statusNotes.Add($Note)
    }
}

function Write-Status {
    param(
        [string]$State,
        [string]$Message
    )

    $lines = @(
        "timestamp=$((Get-Date).ToString("s"))"
        "state=$State"
        "message=$Message"
    )

    foreach ($note in $script:statusNotes) {
        $lines += "note=$note"
    }

    $lines | Out-File -FilePath $statusPath -Encoding utf8
}

function Get-AvailableOutputPath {
    param(
        [string]$PreferredPath,
        [string]$Label
    )

    if (-not (Test-Path $PreferredPath)) {
        return $PreferredPath
    }

    if (Remove-IfExists -Path $PreferredPath) {
        return $PreferredPath
    }

    $directory = Split-Path -Parent $PreferredPath
    $name = [System.IO.Path]::GetFileNameWithoutExtension($PreferredPath)
    $extension = [System.IO.Path]::GetExtension($PreferredPath)
    $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
    $fallbackPath = Join-Path $directory ("{0}-{1}{2}" -f $name, $timestamp, $extension)
    Add-StatusNote "$Label locked; using fallback path: $fallbackPath"
    return $fallbackPath
}

function Get-ScreenshotPath {
    param([string]$Tag)

    $safeTag = $Tag -replace '[^\w\-]+', '-'
    return Join-Path $picturesRoot ("local-{0}.png" -f $safeTag)
}

function Get-DefaultTimeoutSeconds {
    param([string]$CaptureMode)

    switch ($CaptureMode) {
        "EditorRender" { return 180 }
        "EditorPlay" { return 600 }
        "WindowsPlayer" { return 600 }
        default { return 180 }
    }
}

function Get-EffectiveTimeoutSeconds {
    param(
        [string]$CaptureMode,
        [int]$RequestedTimeoutSeconds
    )

    if ($RequestedTimeoutSeconds -gt 0) {
        return $RequestedTimeoutSeconds
    }

    return Get-DefaultTimeoutSeconds -CaptureMode $CaptureMode
}

function Get-UnityProcessesByName {
    param([string[]]$Names)

    return @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $Names -contains $_.ProcessName })
}

function Test-BlockingUnityProcesses {
    param([string]$CaptureMode)

    $fatalNames = @("Unity", "Unity Hub", "UnityPackageManager", "UnityShaderCompiler")
    $fatalProcesses = @(Get-UnityProcessesByName -Names $fatalNames)
    $licensingProcesses = @(Get-UnityProcessesByName -Names @("Unity.Licensing.Client"))

    if ($fatalProcesses.Count -gt 0) {
        $processSummary = ($fatalProcesses |
            Sort-Object ProcessName, Id |
            ForEach-Object { "{0} (PID {1})" -f $_.ProcessName, $_.Id }) -join ", "
        Write-Status -State "blocked" -Message "Blocking Unity-related processes detected: $processSummary"
        throw "Blocking Unity-related processes detected before screenshot run: $processSummary"
    }

    if ($licensingProcesses.Count -gt 0) {
        $processSummary = ($licensingProcesses |
            Sort-Object ProcessName, Id |
            ForEach-Object { "{0} (PID {1})" -f $_.ProcessName, $_.Id }) -join ", "
        Add-StatusNote "Unity.Licensing.Client already running; allowing $CaptureMode because no active Unity editor/player process is blocking."
        Add-StatusNote "licensing_processes=$processSummary"
    }
}

function Start-UnityCliWithTimeout {
    param(
        [string]$FilePath,
        [string[]]$ArgumentList,
        [string]$StartState,
        [string]$StartMessage,
        [string]$FailureState,
        [string]$FailureMessage,
        [string]$TimeoutState,
        [int]$TimeoutSecondsValue,
        [string]$SuccessOutputPath = ""
    )

    Write-Status -State $StartState -Message $StartMessage
    $process = Start-Process -FilePath $FilePath -ArgumentList $ArgumentList -PassThru -NoNewWindow
    $startedAt = Get-Date
    $deadline = $startedAt.AddSeconds($TimeoutSecondsValue)

    while (-not $process.HasExited) {
        if ((Get-Date) -ge $deadline) {
            $elapsedSeconds = [Math]::Round(((Get-Date) - $startedAt).TotalSeconds, 1)
            try {
                Stop-Process -Id $process.Id -Force -ErrorAction Stop
            }
            catch {
                Add-StatusNote "timeout_kill_failed=$($_.Exception.Message)"
            }

            Write-Status -State $TimeoutState -Message "Timed out after $elapsedSeconds seconds."
            throw "Timed out after $elapsedSeconds seconds."
        }

        Start-Sleep -Milliseconds 500
        $process.Refresh()
    }

    if ($process.ExitCode -ne 0) {
        if (-not [string]::IsNullOrWhiteSpace($SuccessOutputPath) -and (Test-Path $SuccessOutputPath)) {
            Add-StatusNote "process_exit_code=$($process.ExitCode)"
            Add-StatusNote "Non-zero exit code ignored because expected output was produced."
            return
        }

        Write-Status -State $FailureState -Message $FailureMessage
        throw $FailureMessage
    }
}

$resolvedUnityPath = Resolve-UnityEditorPath -PreferredPath $UnityPath
$effectiveTimeoutSeconds = Get-EffectiveTimeoutSeconds -CaptureMode $Mode -RequestedTimeoutSeconds $TimeoutSeconds
$screenshotPath = Get-ScreenshotPath -Tag $VerificationTag

Remove-IfExists -Path $statusPath | Out-Null
Write-Status -State "starting" -Message "Preflight started."

$editorLogPath = Get-AvailableOutputPath -PreferredPath $defaultEditorLogPath -Label "build_log"
$runtimeLogPath = Get-AvailableOutputPath -PreferredPath $defaultRuntimeLogPath -Label "runtime_log"
$screenshotPath = Get-AvailableOutputPath -PreferredPath $screenshotPath -Label "screenshot"

Write-Host "Unity editor: $resolvedUnityPath"
Write-Host "Project: $projectPath"
Write-Host "Mode: $Mode"
Write-Host "TimeoutSeconds: $effectiveTimeoutSeconds"
Write-Host "Windows build: $exePath"
Write-Host "Screenshot: $screenshotPath"
Write-Host "Build log: $editorLogPath"
Write-Host "Runtime log: $runtimeLogPath"
Write-Host "Status file: $statusPath"

if ($FailIfUnityProcessesRunning) {
    Test-BlockingUnityProcesses -CaptureMode $Mode
}

if ($Mode -eq "EditorRender") {
    if ($SkipBuild) {
        Add-StatusNote "SkipBuild ignored in EditorRender mode."
    }

    $unityArgs = @(
        "-batchmode",
        "-quit",
        "-projectPath", $projectPath,
        "-executeMethod", "DeepStake.EditorTools.DeepStakeScreenshotCapture.CaptureEditorRenderCli",
        "-logFile", $editorLogPath,
        "-deepstakeVerificationTag=$VerificationTag",
        "-deepstakeScreenshotDir=$screenshotPath",
        "-deepstakeScreenshotTimeoutSeconds=$effectiveTimeoutSeconds"
    )

    Start-UnityCliWithTimeout `
        -FilePath $resolvedUnityPath `
        -ArgumentList $unityArgs `
        -StartState "editor_render_starting" `
        -StartMessage "Starting Unity editor render screenshot capture." `
        -FailureState "editor_render_failed" `
        -FailureMessage "Unity editor render screenshot capture failed. See $editorLogPath" `
        -TimeoutState "editor_render_timeout" `
        -TimeoutSecondsValue $effectiveTimeoutSeconds `
        -SuccessOutputPath $screenshotPath

    if (-not (Test-Path $screenshotPath)) {
        Write-Status -State "editor_render_failed" -Message "Editor render completed without producing screenshot."
        throw "Editor render completed without producing screenshot."
    }

    Write-Status -State "editor_render_success" -Message "Editor render screenshot created successfully: $screenshotPath"
}
elseif ($Mode -eq "EditorPlay") {
    if ($SkipBuild) {
        Add-StatusNote "SkipBuild ignored in EditorPlay mode."
    }

    $unityArgs = @(
        "-batchmode",
        "-projectPath", $projectPath,
        "-executeMethod", "DeepStake.EditorTools.DeepStakeScreenshotCapture.CaptureEditorPlayCli",
        "-logFile", $editorLogPath,
        "-deepstakeVerificationTag=$VerificationTag",
        "-deepstakeScreenshotDir=$screenshotPath",
        "-deepstakeScreenshotTimeoutSeconds=$effectiveTimeoutSeconds"
    )

    Start-UnityCliWithTimeout `
        -FilePath $resolvedUnityPath `
        -ArgumentList $unityArgs `
        -StartState "editor_play_starting" `
        -StartMessage "Starting Unity editor Play Mode screenshot capture." `
        -FailureState "editor_play_failed" `
        -FailureMessage "Unity editor Play Mode screenshot capture failed. See $editorLogPath" `
        -TimeoutState "screenshot_timeout" `
        -TimeoutSecondsValue $effectiveTimeoutSeconds `
        -SuccessOutputPath $screenshotPath

    if (-not (Test-Path $screenshotPath)) {
        Write-Status -State "editor_play_failed" -Message "Editor Play screenshot capture completed without producing screenshot."
        throw "Editor Play screenshot capture completed without producing screenshot."
    }

    Write-Status -State "editor_play_success" -Message "Editor Play screenshot created successfully: $screenshotPath"
}
else {
    if (-not $SkipBuild) {
        $unityArgs = @(
            "-batchmode",
            "-quit",
            "-projectPath", $projectPath,
            "-executeMethod", "DeepStake.EditorTools.DeepStakeAndroidBuild.BuildWindowsDevelopmentCli",
            "-logFile", $editorLogPath
        )

        Start-UnityCliWithTimeout `
            -FilePath $resolvedUnityPath `
            -ArgumentList $unityArgs `
            -StartState "building" `
            -StartMessage "Starting Unity Windows development build." `
            -FailureState "build_failed" `
            -FailureMessage "Unity Windows development build failed. See $editorLogPath" `
            -TimeoutState "screenshot_timeout" `
            -TimeoutSecondsValue $effectiveTimeoutSeconds
    }
    else {
        Write-Status -State "build_skipped" -Message "Skipped build and will use existing executable."
    }

    if (-not (Test-Path $exePath)) {
        Write-Status -State "missing_exe" -Message "Built executable not found: $exePath"
        throw "Built executable not found: $exePath"
    }

    $runtimeArgs = @(
        "-deepstakeAutorunLocalPlay",
        "-deepstakeCaptureScreenshot",
        "-deepstakeQuitAfterScreenshot",
        "-deepstakeVerificationTag=$VerificationTag",
        "-deepstakeScreenshotDir=$screenshotPath",
        "-logFile", $runtimeLogPath
    )

    Write-Status -State "running_player" -Message "Launching existing Windows player for screenshot capture."
    $runProcess = Start-Process -FilePath $exePath -ArgumentList $runtimeArgs -PassThru
    $startedAt = Get-Date
    $deadline = $startedAt.AddSeconds($effectiveTimeoutSeconds)

    while ((Get-Date) -lt $deadline) {
        if (Test-Path $screenshotPath) {
            break
        }

        if ($runProcess.HasExited) {
            break
        }

        Start-Sleep -Milliseconds 500
        $runProcess.Refresh()
    }

    if (-not $runProcess.HasExited) {
        $elapsedSeconds = [Math]::Round(((Get-Date) - $startedAt).TotalSeconds, 1)
        try {
            Stop-Process -Id $runProcess.Id -Force -ErrorAction Stop
        }
        catch {
            Add-StatusNote "runtime_timeout_kill_failed=$($_.Exception.Message)"
        }

        Write-Status -State "screenshot_timeout" -Message "Windows player screenshot timed out after $elapsedSeconds seconds."
        throw "Windows player screenshot timed out after $elapsedSeconds seconds."
    }

    if (-not (Test-Path $screenshotPath)) {
        Write-Status -State "screenshot_timeout" -Message "Screenshot was not produced. Expected: $screenshotPath"
        throw "Screenshot was not produced. Expected: $screenshotPath"
    }

    Write-Status -State "success" -Message "Screenshot created successfully: $screenshotPath"
}

Write-Host ""
Write-Host "DeepStake local screenshot run completed."
Write-Host "Screenshot: $screenshotPath"
Write-Host "Runtime log: $runtimeLogPath"
