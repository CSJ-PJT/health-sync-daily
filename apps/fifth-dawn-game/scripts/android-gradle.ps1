param(
  [Parameter(Mandatory = $true, Position = 0)]
  [string]$GradleTask,

  [Parameter(ValueFromRemainingArguments = $true)]
  [string[]]$GradleArgs
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..\..\..")
$androidDir = Resolve-Path (Join-Path $PSScriptRoot "..\android")

if ($env:FIFTH_DAWN_GRADLE_USER_HOME) {
  $gradleHome = $env:FIFTH_DAWN_GRADLE_USER_HOME
} else {
  $gradleHome = Join-Path $repoRoot ".gradle-local\fifth-dawn-game"
}

New-Item -ItemType Directory -Force -Path $gradleHome | Out-Null
$env:GRADLE_USER_HOME = $gradleHome

Write-Host "Using GRADLE_USER_HOME: $gradleHome"
Set-Location $androidDir

$resolvedTask = if ($GradleTask -eq "stop") { "--stop" } else { $GradleTask }

& .\gradlew.bat $resolvedTask @GradleArgs
exit $LASTEXITCODE
