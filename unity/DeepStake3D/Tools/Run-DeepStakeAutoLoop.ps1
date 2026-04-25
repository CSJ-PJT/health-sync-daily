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
        [string]$RunMode,
        [string]$SuccessText,
        [string]$FailureReason,
        [string]$ScreenshotPathText
    )

    $statusMap = Read-StatusFile
    $lines = @(
        "timestamp=$((Get-Date).ToString("s"))"
        "mode=$RunMode"
        "verification_tag=$VerificationTag"
        "expected_screenshot=$expectedScreenshotPath"
        "screenshot_path=$ScreenshotPathText"
        "success=$SuccessText"
        "failure_reason=$FailureReason"
        "status_file=$statusPath"
        "build_log=$buildLogPath"
        "runtime_log=$runtimeLogPath"
    )

    foreach ($key in @("state", "message")) {
        if ($statusMap.ContainsKey($key)) {
            $lines += "status_$key=$($statusMap[$key])"
        }
    }

    foreach ($note in Get-ValueList -Map $statusMap -Key "note") {
        $lines += "status_note=$note"
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

function Get-BlockingProcesses {
    $names = @("Unity", "Unity Hub", "Unity.Licensing.Client", "UnityPackageManager", "UnityShaderCompiler")
    return @(Get-Process -ErrorAction SilentlyContinue | Where-Object { $names -contains $_.ProcessName })
}

function Stop-BlockingProcesses {
    $blocked = Get-BlockingProcesses
    foreach ($process in $blocked) {
        Stop-Process -Id $process.Id -Force -ErrorAction SilentlyContinue
    }
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

$preflightExitCode = Invoke-ScreenshotScript -RunMode "PreflightOnly"
$preflightStatus = Read-StatusFile

if (($preflightStatus["state"] -ne "success") -and $ForceKillBlockingProcesses) {
    Stop-BlockingProcesses
    Start-Sleep -Seconds 2
    $preflightExitCode = Invoke-ScreenshotScript -RunMode "PreflightOnly"
    $preflightStatus = Read-StatusFile
}

if ($preflightStatus["state"] -ne "success") {
    $reason = if ($preflightStatus.ContainsKey("message")) { $preflightStatus["message"] } else { "PreflightOnly failed." }
    Write-RunReport -RunMode "PreflightOnly" -SuccessText "false" -FailureReason $reason -ScreenshotPathText ""
    Write-Host ""
    Write-Host "DeepStake auto loop summary"
    Write-Host "Mode: PreflightOnly"
    Write-Host "Success: false"
    Write-Host "Failure: $reason"
    Write-Host "Status: $statusPath"
    Write-Host "Report: $reportPath"
    exit 1
}

if ($Mode -eq "PreflightOnly") {
    Write-RunReport -RunMode "PreflightOnly" -SuccessText "true" -FailureReason "" -ScreenshotPathText ""
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

if ($screenshotExists -or $statusAfterRun["state"] -eq "editor_render_success" -or $statusAfterRun["state"] -eq "editor_play_success" -or $statusAfterRun["state"] -eq "success") {
    Publish-LatestAlias
    Write-RunReport -RunMode $Mode -SuccessText "true" -FailureReason "" -ScreenshotPathText $expectedScreenshotPath
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
Write-RunReport -RunMode $Mode -SuccessText "false" -FailureReason $failureReason -ScreenshotPathText ""
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
