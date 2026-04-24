param(
    [string]$UnityPath,
    [switch]$SkipBuild,
    [string]$VerificationTag = "local-dev-run",
    [int]$LaunchTimeoutSeconds = 90
)

$ErrorActionPreference = "Stop"

$projectPath = Split-Path -Parent $PSScriptRoot
$repoRoot = Split-Path -Parent $projectPath
$windowsBuildRoot = Join-Path $projectPath "Builds\Windows"
$verificationRoot = Join-Path $projectPath "TestResults"
$picturesRoot = Join-Path $projectPath "Pictures\Screenshot"
$editorLogPath = Join-Path $verificationRoot "local-windows-build.log"
$runtimeLogPath = Join-Path $verificationRoot "local-windows-runtime.log"
$exePath = Join-Path $windowsBuildRoot "DeepStake-local-dev.exe"

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
        Remove-Item -LiteralPath $Path -Force
    }
}

function Get-ScreenshotPath {
    param([string]$Tag)

    $safeTag = $Tag -replace '[^\w\-]+', '-'
    return Join-Path $picturesRoot ("local-{0}.png" -f $safeTag)
}

$resolvedUnityPath = Resolve-UnityEditorPath -PreferredPath $UnityPath
$screenshotPath = Get-ScreenshotPath -Tag $VerificationTag

Remove-IfExists -Path $editorLogPath
Remove-IfExists -Path $runtimeLogPath
Remove-IfExists -Path $screenshotPath

Write-Host "Unity editor: $resolvedUnityPath"
Write-Host "Project: $projectPath"
Write-Host "Windows build: $exePath"
Write-Host "Screenshot: $screenshotPath"
Write-Host "Build log: $editorLogPath"
Write-Host "Runtime log: $runtimeLogPath"

if (-not $SkipBuild) {
    $unityArgs = @(
        "-batchmode",
        "-quit",
        "-projectPath", $projectPath,
        "-executeMethod", "DeepStake.EditorTools.DeepStakeAndroidBuild.BuildWindowsDevelopmentCli",
        "-logFile", $editorLogPath
    )

    $buildProcess = Start-Process -FilePath $resolvedUnityPath -ArgumentList $unityArgs -PassThru -Wait -NoNewWindow
    if ($buildProcess.ExitCode -ne 0) {
        throw "Unity Windows development build failed. See $editorLogPath"
    }
}

if (-not (Test-Path $exePath)) {
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

$runProcess = Start-Process -FilePath $exePath -ArgumentList $runtimeArgs -PassThru
$deadline = (Get-Date).AddSeconds($LaunchTimeoutSeconds)

while ((Get-Date) -lt $deadline) {
    if (Test-Path $screenshotPath) {
        break
    }

    Start-Sleep -Milliseconds 500
}

if ($runProcess -and -not $runProcess.HasExited) {
    $runProcess.WaitForExit([Math]::Max(5000, $LaunchTimeoutSeconds * 1000)) | Out-Null
}

if (-not (Test-Path $screenshotPath)) {
    throw "Screenshot was not produced within timeout. Expected: $screenshotPath"
}

Write-Host ""
Write-Host "DeepStake local Windows screenshot run completed."
Write-Host "Screenshot: $screenshotPath"
Write-Host "Runtime log: $runtimeLogPath"
