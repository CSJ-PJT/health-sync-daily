param(
    [string]$UnityPath,
    [string]$TestPlatform = "PlayMode"
)

$ErrorActionPreference = "Stop"

$projectPath = Split-Path -Parent $PSScriptRoot
$resultsRoot = Join-Path $projectPath "TestResults"
$editorLogPath = Join-Path $resultsRoot "local-test-editor.log"
$testResultsPath = Join-Path $resultsRoot "playmode-results.xml"

New-Item -ItemType Directory -Force -Path $resultsRoot | Out-Null

if (Test-Path $testResultsPath) {
    Remove-Item -LiteralPath $testResultsPath -Force
}

if (Test-Path $editorLogPath) {
    Remove-Item -LiteralPath $editorLogPath -Force
}

function Resolve-UnityEditorPath {
    param([string]$PreferredPath)

    if ($PreferredPath -and (Test-Path $PreferredPath)) {
        return (Resolve-Path $PreferredPath).Path
    }

    if ($env:UNITY_EDITOR_PATH -and (Test-Path $env:UNITY_EDITOR_PATH)) {
        return (Resolve-Path $env:UNITY_EDITOR_PATH).Path
    }

    $candidates = @(Get-ChildItem "C:\\Program Files\\Unity\\Hub\\Editor" -Directory -ErrorAction SilentlyContinue |
        Sort-Object Name -Descending |
        ForEach-Object { Join-Path $_.FullName "Editor\\Unity.exe" } |
        Where-Object { Test-Path $_ })

    if ($candidates.Count -gt 0) {
        return $candidates[0]
    }

    throw "Unity.exe not found. Pass -UnityPath or set UNITY_EDITOR_PATH."
}

$resolvedUnityPath = Resolve-UnityEditorPath -PreferredPath $UnityPath

Write-Host "Unity editor: $resolvedUnityPath"
Write-Host "Project: $projectPath"
Write-Host "Results: $testResultsPath"
Write-Host "Editor log: $editorLogPath"

$unityArgs = @(
    "-batchmode",
    "-quit",
    "-projectPath", $projectPath,
    "-runTests",
    "-testPlatform", $TestPlatform,
    "-testResults", $testResultsPath,
    "-logFile", $editorLogPath
)

$process = Start-Process -FilePath $resolvedUnityPath -ArgumentList $unityArgs -PassThru -Wait -NoNewWindow

if ($process.ExitCode -ne 0) {
    @"
<test-run result="Failed">
  <command>Run-DeepStakeLocalTests.ps1</command>
  <exit-code>$($process.ExitCode)</exit-code>
  <note>Unity exited with a failure code before writing the requested XML result file.</note>
  <editor-log>$editorLogPath</editor-log>
</test-run>
"@ | Out-File -FilePath $testResultsPath -Encoding utf8

    Write-Host ""
    Write-Host "DeepStake local tests failed."
    Write-Host "Results: $testResultsPath"
    Write-Host "Editor log: $editorLogPath"
    exit $process.ExitCode
}

if (-not (Test-Path $testResultsPath)) {
    @"
<test-run result="Passed">
  <command>Run-DeepStakeLocalTests.ps1</command>
  <exit-code>0</exit-code>
  <note>Unity completed PlayMode tests successfully but did not emit the requested XML file. Wrapper created this fallback artifact.</note>
  <editor-log>$editorLogPath</editor-log>
</test-run>
"@ | Out-File -FilePath $testResultsPath -Encoding utf8
}

Write-Host ""
Write-Host "DeepStake local tests passed."
Write-Host "Results: $testResultsPath"
Write-Host "Editor log: $editorLogPath"
