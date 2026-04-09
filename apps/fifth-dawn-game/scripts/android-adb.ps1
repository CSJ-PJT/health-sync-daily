param(
  [Parameter(Mandatory = $true)]
  [ValidateSet("devices")]
  [string]$Command
)

$ErrorActionPreference = "Stop"

function Resolve-AdbPath {
  if ($env:ANDROID_SDK_ROOT) {
    $candidate = Join-Path $env:ANDROID_SDK_ROOT "platform-tools\adb.exe"
    if (Test-Path $candidate) {
      return $candidate
    }
  }

  if ($env:ANDROID_HOME) {
    $candidate = Join-Path $env:ANDROID_HOME "platform-tools\adb.exe"
    if (Test-Path $candidate) {
      return $candidate
    }
  }

  if ($env:LOCALAPPDATA) {
    $candidate = Join-Path $env:LOCALAPPDATA "Android\Sdk\platform-tools\adb.exe"
    if (Test-Path $candidate) {
      return $candidate
    }
  }

  $adbFromPath = Get-Command adb.exe -ErrorAction SilentlyContinue
  if ($adbFromPath) {
    return $adbFromPath.Source
  }

  throw "adb.exe not found. Set ANDROID_SDK_ROOT or install Android platform-tools."
}

$adb = Resolve-AdbPath
Write-Host "Using adb: $adb"

switch ($Command) {
  "devices" {
    & $adb devices
    exit $LASTEXITCODE
  }
}
