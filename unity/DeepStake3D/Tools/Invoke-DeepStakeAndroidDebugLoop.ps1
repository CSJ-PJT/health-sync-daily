param(
    [string]$UnityPath,
    [string]$DeviceSerial,
    [int]$StartupDelaySeconds = 12,
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"

$projectPath = Split-Path -Parent $PSScriptRoot
$buildRoot = Join-Path $projectPath "Builds\\Android"
$artifactsRoot = Join-Path $buildRoot "Verification"
$apkPath = Join-Path $buildRoot "DeepStake-debug.apk"
$unityLogPath = Join-Path $artifactsRoot "unity-cli-build.log"
$packageName = "com.roboheart.deepstake"
$activityName = "com.unity3d.player.UnityPlayerGameActivity"
$timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
$tag = "android-debug-$timestamp"
$screenshotPath = Join-Path $artifactsRoot "device-$timestamp.png"
$logPath = Join-Path $artifactsRoot "logcat-$timestamp.txt"
$logExcerptPath = Join-Path $artifactsRoot "logcat-$timestamp-deepstake.txt"

New-Item -ItemType Directory -Force -Path $artifactsRoot | Out-Null

function Resolve-AdbPath {
    $command = Get-Command adb -ErrorAction SilentlyContinue
    if ($command) {
        return $command.Source
    }

    $candidates = @()
    if ($env:LOCALAPPDATA) {
        $candidates += Join-Path $env:LOCALAPPDATA "Android\\Sdk\\platform-tools\\adb.exe"
    }
    if ($env:ANDROID_SDK_ROOT) {
        $candidates += Join-Path $env:ANDROID_SDK_ROOT "platform-tools\\adb.exe"
    }
    if ($env:ANDROID_HOME) {
        $candidates += Join-Path $env:ANDROID_HOME "platform-tools\\adb.exe"
    }

    $candidates = @($candidates | Where-Object { $_ -and (Test-Path $_) })

    if ($candidates.Count -gt 0) {
        return $candidates[0]
    }

    throw "adb.exe not found. Install Android platform-tools or add adb to PATH."
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

function Invoke-Adb {
    param(
        [Parameter(ValueFromRemainingArguments = $true)]
        [string[]]$Args
    )

    $command = @()
    if ($script:ResolvedDeviceSerial) {
        $command += "-s"
        $command += $script:ResolvedDeviceSerial
    }
    $command += $Args

    & $script:ResolvedAdbPath @command
    if ($LASTEXITCODE -ne 0) {
        throw "adb command failed: $script:ResolvedAdbPath $($command -join ' ')"
    }
}

function Resolve-DeviceSerial {
    param([string]$RequestedSerial)

    $lines = & $script:ResolvedAdbPath devices
    if ($LASTEXITCODE -ne 0) {
        throw "adb devices failed."
    }

    $devices = @($lines | Select-Object -Skip 1 | Where-Object { $_ -match "\tdevice$" } | ForEach-Object {
        ($_ -split "\t")[0].Trim()
    })

    if ($RequestedSerial) {
        if ($devices -notcontains $RequestedSerial) {
            throw "Requested device serial not available: $RequestedSerial"
        }

        return $RequestedSerial
    }

    if ($devices.Count -eq 0) {
        throw "No adb device in 'device' state."
    }

    return $devices[0]
}

function Resolve-PrimaryDisplayId {
    $displayLines = & $script:ResolvedAdbPath -s $script:ResolvedDeviceSerial shell dumpsys SurfaceFlinger --display-id
    if ($LASTEXITCODE -ne 0) {
        return $null
    }

    foreach ($line in $displayLines) {
        if ($line -match '^Display\s+(\d+)') {
            return $matches[1]
        }
    }

    return $null
}

function Resolve-AllDisplayIds {
    $displayLines = & $script:ResolvedAdbPath -s $script:ResolvedDeviceSerial shell dumpsys SurfaceFlinger --display-id
    if ($LASTEXITCODE -ne 0) {
        return @()
    }

    $displayIds = @()
    foreach ($line in $displayLines) {
        if ($line -match '^Display\s+(\d+)') {
            $displayIds += $matches[1]
        }
    }

    return @($displayIds | Select-Object -Unique)
}

function Capture-DisplayScreenshot {
    param(
        [string]$DisplayId,
        [string]$OutputPath
    )

    $screenshotErrorPath = "$OutputPath.stderr.txt"
    $screenshotArgs = @("-s", $script:ResolvedDeviceSerial, "exec-out", "screencap", "-p")
    if ($DisplayId) {
        $screenshotArgs += @("-d", $DisplayId)
    }

    $screenshotProcess = Start-Process `
        -FilePath $script:ResolvedAdbPath `
        -ArgumentList $screenshotArgs `
        -RedirectStandardOutput $OutputPath `
        -RedirectStandardError $screenshotErrorPath `
        -PassThru `
        -Wait `
        -NoNewWindow

    if ($screenshotProcess.ExitCode -ne 0) {
        throw "adb exec-out screencap failed with exit code $($screenshotProcess.ExitCode)."
    }

    if (-not (Test-Path $OutputPath) -or (Get-Item $OutputPath).Length -le 0) {
        throw "Screenshot file was not created: $OutputPath"
    }
}

$script:ResolvedAdbPath = Resolve-AdbPath
$resolvedUnityPath = Resolve-UnityEditorPath -PreferredPath $UnityPath
$script:ResolvedDeviceSerial = Resolve-DeviceSerial -RequestedSerial $DeviceSerial
$script:ResolvedDisplayId = Resolve-PrimaryDisplayId
$script:ResolvedDisplayIds = Resolve-AllDisplayIds

Write-Host "Unity editor: $resolvedUnityPath"
Write-Host "adb path: $script:ResolvedAdbPath"
Write-Host "adb device: $script:ResolvedDeviceSerial"
if ($script:ResolvedDisplayId) {
    Write-Host "display id: $script:ResolvedDisplayId"
}
Write-Host "Project: $projectPath"

if (-not $SkipBuild) {
    Write-Host "Running Unity CLI Android debug build..."
    $unityArgs = @(
        "-quit",
        "-batchmode",
        "-projectPath", $projectPath,
        "-executeMethod", "DeepStake.EditorTools.DeepStakeAndroidBuild.BuildAndroidDebugCli",
        "-logFile", $unityLogPath
    )

    $unityProcess = Start-Process -FilePath $resolvedUnityPath -ArgumentList $unityArgs -PassThru -Wait -NoNewWindow
    if ($unityProcess.ExitCode -ne 0) {
        throw "Unity CLI build failed with exit code $($unityProcess.ExitCode). See $unityLogPath"
    }
}

if (-not (Test-Path $apkPath)) {
    throw "APK not found: $apkPath"
}

Write-Host "Clearing existing logcat buffer..."
Invoke-Adb logcat -c

Write-Host "Installing APK..."
Invoke-Adb install -r $apkPath

Write-Host "Stopping existing app process..."
Invoke-Adb -Args @("shell", "am", "force-stop", $packageName)

Write-Host "Launching app from launcher entry..."
Invoke-Adb -Args @("shell", "monkey", "-p", $packageName, "-c", "android.intent.category.LAUNCHER", "1")

Write-Host "Waiting $StartupDelaySeconds seconds for startup..."
Start-Sleep -Seconds $StartupDelaySeconds

Write-Host "Capturing device screenshot..."
$capturedDisplayPaths = @()
if ($script:ResolvedDisplayIds.Count -gt 0) {
    foreach ($displayId in $script:ResolvedDisplayIds) {
        $displayScreenshotPath = Join-Path $artifactsRoot "device-$timestamp-display-$displayId.png"
        Capture-DisplayScreenshot -DisplayId $displayId -OutputPath $displayScreenshotPath
        $capturedDisplayPaths += $displayScreenshotPath
    }
}
else {
    Capture-DisplayScreenshot -DisplayId $null -OutputPath $screenshotPath
    $capturedDisplayPaths += $screenshotPath
}

if ($capturedDisplayPaths.Count -gt 0) {
    $preferredScreenshot = $capturedDisplayPaths |
        Sort-Object { (Get-Item $_).Length } -Descending |
        Select-Object -First 1

    if ($preferredScreenshot) {
        Copy-Item -Path $preferredScreenshot -Destination $screenshotPath -Force
    }
}

Write-Host "Capturing logcat..."
& $script:ResolvedAdbPath -s $script:ResolvedDeviceSerial logcat -d -v time "Unity:D" "*:S" | Out-File -FilePath $logPath -Encoding utf8
if ($LASTEXITCODE -ne 0) {
    throw "adb logcat capture failed."
}

$deepStakeLines = Select-String -Path $logPath -Pattern "DeepStakeDev|DeepStakeBuild|WorldPrototype3D|MainMenu|Boot" -SimpleMatch:$false
if ($deepStakeLines) {
    $deepStakeLines | ForEach-Object { $_.Line } | Out-File -FilePath $logExcerptPath -Encoding utf8
}
else {
    "No DeepStake-specific lines found in Unity logcat filter." | Out-File -FilePath $logExcerptPath -Encoding utf8
}

Write-Host ""
Write-Host "Android debug loop complete."
Write-Host "APK: $apkPath"
Write-Host "Unity build log: $unityLogPath"
Write-Host "Screenshot: $screenshotPath"
Write-Host "Logcat: $logPath"
Write-Host "Log excerpt: $logExcerptPath"
