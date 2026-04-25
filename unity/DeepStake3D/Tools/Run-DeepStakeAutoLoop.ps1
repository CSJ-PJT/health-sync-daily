param(
    [string]$VerificationTag = "auto-test",
    [ValidateSet("PreflightOnly", "EditorRender", "EditorPlay", "WindowsPlayer")]
    [string]$Mode = "EditorRender",
    [switch]$ForceKillBlockingProcesses,
    [int]$TimeoutSeconds = 0,
    [switch]$KeepPreviousLatest,
    [switch]$NoLatestAlias
)

$ErrorActionPreference = "Stop"

$projectPath = Split-Path -Parent $PSScriptRoot
$screenshotScriptPath = Join-Path $PSScriptRoot "Invoke-DeepStakeLocalScreenshotRun.ps1"
$testResultsPath = Join-Path $projectPath "TestResults"
$statusPath = Join-Path $testResultsPath "local-windows-screenshot-status.txt"
$reportPath = Join-Path $testResultsPath "local-auto-loop-report.txt"
$buildLogPath = Join-Path $testResultsPath "local-windows-build.log"
$runtimeLogPath = Join-Path $testResultsPath "local-windows-runtime.log"
$screenshotRoot = Join-Path $projectPath "Pictures\Screenshot"
$expectedScreenshotPath = Join-Path $screenshotRoot ("local-{0}.png" -f ($VerificationTag -replace '[^\w\-]+', '-'))
$latestAliasPath = Join-Path $screenshotRoot "latest-local.png"
$previousLatestAliasPath = Join-Path $screenshotRoot "latest-local.previous.png"
$unityProcessNames = @("Unity", "Unity Hub", "Unity.Licensing.Client", "UnityPackageManager", "UnityShaderCompiler")

New-Item -ItemType Directory -Force -Path $testResultsPath | Out-Null
New-Item -ItemType Directory -Force -Path $screenshotRoot | Out-Null

function Read-StatusFile {
    if (-not (Test-Path $statusPath)) {
        return @{}
    }

    $map = @{}
    foreach ($line in Get-Content -LiteralPath $statusPath -ErrorAction SilentlyContinue) {
        $parts = $line -split "=", 2
        if ($parts.Count -eq 2) {
            if ($map.ContainsKey($parts[0])) {
                if ($map[$parts[0]] -is [System.Collections.IList]) {
                    $map[$parts[0]].Add($parts[1]) | Out-Null
                }
                else {
                    $list = New-Object System.Collections.ArrayList
                    [void]$list.Add($map[$parts[0]])
                    [void]$list.Add($parts[1])
                    $map[$parts[0]] = $list
                }
            }
            else {
                $map[$parts[0]] = $parts[1]
            }
        }
    }

    return $map
}

function Get-ValueList {
    param(
        [hashtable]$Map,
        [string]$Key
    )

    if (-not $Map.ContainsKey($Key)) {
        return @()
    }

    $value = $Map[$Key]
    if ($value -is [System.Collections.IList]) {
        return @($value)
    }

    return @($value)
}

function Write-RunReport {
    param(
        [hashtable]$Fields
    )

    $lines = foreach ($key in $Fields.Keys) {
        "{0}={1}" -f $key, $Fields[$key]
    }

    $lines | Out-File -LiteralPath $reportPath -Encoding utf8
}

function Invoke-ScreenshotScript {
    param(
        [string]$RunMode
    )

    $arguments = @(
        "-ExecutionPolicy", "Bypass",
        "-File", $screenshotScriptPath,
        "-Mode", $RunMode,
        "-VerificationTag", $VerificationTag
    )

    if ($TimeoutSeconds -gt 0) {
        $arguments += @("-TimeoutSeconds", $TimeoutSeconds)
    }

    & powershell @arguments
    return $LASTEXITCODE
}

function Get-UnityProcesses {
    return @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $unityProcessNames -contains $_.ProcessName })
}

function Stop-UnityProcesses {
    $blocked = Get-UnityProcesses
    $killed = New-Object System.Collections.Generic.List[string]
    foreach ($process in $blocked) {
        $label = "{0} (PID {1})" -f $process.ProcessName, $process.Id
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
        $killed.Add($label)
    }
    return @($killed)
}

function Join-ValueList {
    param([string[]]$Values)

    if (-not $Values -or $Values.Count -eq 0) {
        return ""
    }

    return ($Values -join "; ")
}

function Should-RetryEnvironmentFailure {
    param(
        [hashtable]$StatusMap,
        [bool]$ScreenshotExists
    )

    if ($ScreenshotExists) {
        return $false
    }

    foreach ($note in Get-ValueList -Map $StatusMap -Key "note") {
        if ($note -match "^root_cause_category=(.+)$") {
            return @("database_lock", "readonly_database", "upm_ipc_failure", "licensing_mutex") -contains $matches[1]
        }
    }

    return $false
}

function Get-RootCauseCategory {
    param([hashtable]$StatusMap)

    foreach ($note in Get-ValueList -Map $StatusMap -Key "note") {
        if ($note -match "^root_cause_category=(.+)$") {
            return $matches[1]
        }
    }

    return ""
}

function Get-StatusMessage {
    param([hashtable]$StatusMap)

    if ($StatusMap.ContainsKey("message")) {
        return $StatusMap["message"]
    }

    return ""
}

function Publish-LatestAlias {
    if ($NoLatestAlias -or -not (Test-Path $expectedScreenshotPath)) {
        return
    }

    if ($KeepPreviousLatest -and (Test-Path $latestAliasPath)) {
        Copy-Item -LiteralPath $latestAliasPath -Destination $previousLatestAliasPath -Force
    }

    Copy-Item -LiteralPath $expectedScreenshotPath -Destination $latestAliasPath -Force
}

$preCleanupAttempted = $false
$preCleanupProcessesKilled = @()
$preflightBeforeCapture = ""
$firstCaptureResult = ""
$retryAttempted = $false
$retryReason = ""
$retryCaptureResult = ""
$postCleanupAttempted = $false
$postCleanupProcessesKilled = @()
$finalResult = "failure"
$failureReason = ""
$finalScreenshotExists = $false
$statusState = ""
$statusMessage = ""
$statusRootCause = ""
$statusNotes = @()

if ($ForceKillBlockingProcesses) {
    $preCleanupAttempted = $true
    $preCleanupProcessesKilled = Stop-UnityProcesses
    Start-Sleep -Seconds 10
}

$preflightExitCode = Invoke-ScreenshotScript -RunMode "PreflightOnly"
$preflightStatus = Read-StatusFile
$preflightBeforeCapture = if ($preflightStatus.ContainsKey("state")) { $preflightStatus["state"] } else { "missing_status" }

if ($preflightBeforeCapture -ne "success") {
    $failureReason = if ($preflightStatus.ContainsKey("message")) { $preflightStatus["message"] } else { "PreflightOnly failed." }
    $statusState = $preflightBeforeCapture
    $statusMessage = Get-StatusMessage -StatusMap $preflightStatus
    $statusRootCause = Get-RootCauseCategory -StatusMap $preflightStatus
    $statusNotes = Get-ValueList -Map $preflightStatus -Key "note"
    Write-RunReport -Fields @{
        timestamp = (Get-Date).ToString("s")
        mode = "PreflightOnly"
        verification_tag = $VerificationTag
        expected_screenshot = $expectedScreenshotPath
        screenshot_path = ""
        success = "false"
        failure_reason = $failureReason
        status_file = $statusPath
        build_log = $buildLogPath
        runtime_log = $runtimeLogPath
        pre_cleanup_attempted = $preCleanupAttempted.ToString().ToLowerInvariant()
        pre_cleanup_processes_killed = (Join-ValueList -Values $preCleanupProcessesKilled)
        preflight_before_capture = $preflightBeforeCapture
        first_capture_result = ""
        retry_attempted = "false"
        retry_reason = ""
        retry_capture_result = ""
        post_cleanup_attempted = "false"
        post_cleanup_processes_killed = ""
        final_screenshot_exists = "false"
        final_result = "failure"
        status_state = $statusState
        status_message = $statusMessage
        status_root_cause_category = $statusRootCause
        status_notes = (Join-ValueList -Values $statusNotes)
    }
    Write-Host ""
    Write-Host "DeepStake auto loop summary"
    Write-Host "Mode: PreflightOnly"
    Write-Host "Success: false"
    Write-Host "Failure: $failureReason"
    Write-Host "Status: $statusPath"
    Write-Host "Report: $reportPath"
    exit 1
}

if ($Mode -eq "PreflightOnly") {
    Write-RunReport -Fields @{
        timestamp = (Get-Date).ToString("s")
        mode = "PreflightOnly"
        verification_tag = $VerificationTag
        expected_screenshot = $expectedScreenshotPath
        screenshot_path = ""
        success = "true"
        failure_reason = ""
        status_file = $statusPath
        build_log = $buildLogPath
        runtime_log = $runtimeLogPath
        pre_cleanup_attempted = $preCleanupAttempted.ToString().ToLowerInvariant()
        pre_cleanup_processes_killed = (Join-ValueList -Values $preCleanupProcessesKilled)
        preflight_before_capture = $preflightBeforeCapture
        first_capture_result = ""
        retry_attempted = "false"
        retry_reason = ""
        retry_capture_result = ""
        post_cleanup_attempted = "false"
        post_cleanup_processes_killed = ""
        final_screenshot_exists = "false"
        final_result = "success"
        status_state = "success"
        status_message = "PreflightOnly succeeded."
        status_root_cause_category = ""
        status_notes = ""
    }
    Write-Host ""
    Write-Host "DeepStake auto loop summary"
    Write-Host "Mode: PreflightOnly"
    Write-Host "Success: true"
    Write-Host "Status: $statusPath"
    Write-Host "Report: $reportPath"
    exit 0
}

$runExitCode = Invoke-ScreenshotScript -RunMode $Mode
$statusAfterRun = Read-StatusFile
$screenshotExists = Test-Path $expectedScreenshotPath
$firstCaptureResult = if ($screenshotExists) { "success" } elseif ($statusAfterRun.ContainsKey("state")) { $statusAfterRun["state"] } else { "missing_status" }

if ((-not $screenshotExists) -and (Should-RetryEnvironmentFailure -StatusMap $statusAfterRun -ScreenshotExists $screenshotExists)) {
    $retryAttempted = $true
    $retryReason = Get-RootCauseCategory -StatusMap $statusAfterRun
    $retryCleanupKilled = Stop-UnityProcesses
    if ($retryCleanupKilled.Count -gt 0) {
        $preCleanupProcessesKilled += $retryCleanupKilled
    }
    Start-Sleep -Seconds 10
    $retryExitCode = Invoke-ScreenshotScript -RunMode $Mode
    $statusAfterRun = Read-StatusFile
    $screenshotExists = Test-Path $expectedScreenshotPath
    $retryCaptureResult = if ($screenshotExists) { "success" } elseif ($statusAfterRun.ContainsKey("state")) { $statusAfterRun["state"] } else { "missing_status" }
}

if ($ForceKillBlockingProcesses) {
    $postCleanupAttempted = $true
    $postCleanupProcessesKilled = Stop-UnityProcesses
    Start-Sleep -Seconds 5
}

$finalScreenshotExists = Test-Path $expectedScreenshotPath
$statusState = if ($statusAfterRun.ContainsKey("state")) { $statusAfterRun["state"] } else { "" }
$statusMessage = Get-StatusMessage -StatusMap $statusAfterRun
$statusRootCause = Get-RootCauseCategory -StatusMap $statusAfterRun
$statusNotes = Get-ValueList -Map $statusAfterRun -Key "note"

if ($finalScreenshotExists -or $statusState -eq "editor_render_success" -or $statusState -eq "editor_play_success" -or $statusState -eq "success") {
    Publish-LatestAlias
    $finalResult = "success"
    Write-RunReport -Fields @{
        timestamp = (Get-Date).ToString("s")
        mode = $Mode
        verification_tag = $VerificationTag
        expected_screenshot = $expectedScreenshotPath
        screenshot_path = $expectedScreenshotPath
        success = "true"
        failure_reason = ""
        status_file = $statusPath
        build_log = $buildLogPath
        runtime_log = $runtimeLogPath
        pre_cleanup_attempted = $preCleanupAttempted.ToString().ToLowerInvariant()
        pre_cleanup_processes_killed = (Join-ValueList -Values $preCleanupProcessesKilled)
        preflight_before_capture = $preflightBeforeCapture
        first_capture_result = $firstCaptureResult
        retry_attempted = $retryAttempted.ToString().ToLowerInvariant()
        retry_reason = $retryReason
        retry_capture_result = $retryCaptureResult
        post_cleanup_attempted = $postCleanupAttempted.ToString().ToLowerInvariant()
        post_cleanup_processes_killed = (Join-ValueList -Values $postCleanupProcessesKilled)
        final_screenshot_exists = $finalScreenshotExists.ToString().ToLowerInvariant()
        final_result = $finalResult
        status_state = $statusState
        status_message = $statusMessage
        status_root_cause_category = $statusRootCause
        status_notes = (Join-ValueList -Values $statusNotes)
    }
    Write-Host ""
    Write-Host "DeepStake auto loop summary"
    Write-Host "Mode: $Mode"
    Write-Host "Success: true"
    Write-Host "Screenshot: $expectedScreenshotPath"
    if (-not $NoLatestAlias) {
        Write-Host "Latest alias: $latestAliasPath"
    }
    Write-Host "Status: $statusPath"
    Write-Host "Report: $reportPath"
    exit 0
}

$failureReason = if ($statusAfterRun.ContainsKey("message")) { $statusAfterRun["message"] } else { "Screenshot was not generated." }
Write-RunReport -Fields @{
    timestamp = (Get-Date).ToString("s")
    mode = $Mode
    verification_tag = $VerificationTag
    expected_screenshot = $expectedScreenshotPath
    screenshot_path = ""
    success = "false"
    failure_reason = $failureReason
    status_file = $statusPath
    build_log = $buildLogPath
    runtime_log = $runtimeLogPath
    pre_cleanup_attempted = $preCleanupAttempted.ToString().ToLowerInvariant()
    pre_cleanup_processes_killed = (Join-ValueList -Values $preCleanupProcessesKilled)
    preflight_before_capture = $preflightBeforeCapture
    first_capture_result = $firstCaptureResult
    retry_attempted = $retryAttempted.ToString().ToLowerInvariant()
    retry_reason = $retryReason
    retry_capture_result = $retryCaptureResult
    post_cleanup_attempted = $postCleanupAttempted.ToString().ToLowerInvariant()
    post_cleanup_processes_killed = (Join-ValueList -Values $postCleanupProcessesKilled)
    final_screenshot_exists = $finalScreenshotExists.ToString().ToLowerInvariant()
    final_result = $finalResult
    status_state = $statusState
    status_message = $statusMessage
    status_root_cause_category = $statusRootCause
    status_notes = (Join-ValueList -Values $statusNotes)
}
Write-Host ""
Write-Host "DeepStake auto loop summary"
Write-Host "Mode: $Mode"
Write-Host "Success: false"
Write-Host "Failure: $failureReason"
Write-Host "Status: $statusPath"
Write-Host "Build log: $buildLogPath"
Write-Host "Runtime log: $runtimeLogPath"
Write-Host "Report: $reportPath"
exit 1
