using System.IO;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace DeepStake.EditorTools
{
    public static class DeepStakeAndroidBuild
    {
        private const string AndroidPackageName = "com.roboheart.deepstake";
        private const string DebugApkName = "DeepStake-debug.apk";
        private const string WindowsExeName = "DeepStake-local-dev.exe";
        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/WorldPrototype3D.unity",
        };

        public static void BuildAndroidDebug()
        {
            var report = BuildAndroidDebugInternal(DebugApkName);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException("DeepStake Android build failed: " + report.summary.result);
            }
        }

        public static void BuildAndroidDebugCli()
        {
            try
            {
                Debug.Log("[DeepStakeBuild] Starting Android debug CLI build.");
                var report = BuildAndroidDebugInternal(DebugApkName);
                if (report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError("[DeepStakeBuild] Android debug CLI build failed: " + report.summary.result);
                    EditorApplication.Exit(1);
                    return;
                }

                Debug.Log("[DeepStakeBuild] Android debug CLI build succeeded. apk=" +
                          Path.GetFullPath(Path.Combine("Builds", "Android", DebugApkName)) +
                          " sizeBytes=" + report.summary.totalSize +
                          " durationSeconds=" + report.summary.totalTime.TotalSeconds);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError("[DeepStakeBuild] Android debug CLI build threw: " + exception);
                EditorApplication.Exit(1);
            }
        }

        public static void BuildWindowsDevelopmentCli()
        {
            try
            {
                Debug.Log("[DeepStakeBuild] Starting Windows development CLI build.");
                var report = BuildWindowsDevelopmentInternal(WindowsExeName);
                if (report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError("[DeepStakeBuild] Windows development CLI build failed: " + report.summary.result);
                    EditorApplication.Exit(1);
                    return;
                }

                Debug.Log("[DeepStakeBuild] Windows development CLI build succeeded. exe=" +
                          Path.GetFullPath(Path.Combine("Builds", "Windows", WindowsExeName)) +
                          " sizeBytes=" + report.summary.totalSize +
                          " durationSeconds=" + report.summary.totalTime.TotalSeconds);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError("[DeepStakeBuild] Windows development CLI build threw: " + exception);
                EditorApplication.Exit(1);
            }
        }

        private static BuildReport BuildAndroidDebugInternal(string apkName)
        {
            var buildRoot = Path.GetFullPath(Path.Combine("Builds", "Android"));
            Directory.CreateDirectory(buildRoot);
            var outputPath = Path.Combine(buildRoot, apkName);

            ValidateScenes();
            ApplyAndroidIdentity();
            SwitchToAndroidIfNeeded();
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.connectProfiler = false;
            EditorUserBuildSettings.allowDebugging = true;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            TrySetAndroidCreateSymbolsDisabled();

            var options = new BuildPlayerOptions
            {
                scenes = ScenePaths,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development | BuildOptions.CompressWithLz4
            };

            Debug.Log("[DeepStakeBuild] Building package=" + AndroidPackageName + " output=" + outputPath);
            return BuildPipeline.BuildPlayer(options);
        }

        private static BuildReport BuildWindowsDevelopmentInternal(string exeName)
        {
            var buildRoot = Path.GetFullPath(Path.Combine("Builds", "Windows"));
            Directory.CreateDirectory(buildRoot);
            var outputPath = Path.Combine(buildRoot, exeName);

            ValidateScenes();
            SwitchToWindowsIfNeeded();

            var options = new BuildPlayerOptions
            {
                scenes = ScenePaths,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.CompressWithLz4
            };

            Debug.Log("[DeepStakeBuild] Building Windows development player output=" + outputPath);
            return BuildPipeline.BuildPlayer(options);
        }

        [MenuItem("DeepStake/Android/Apply Fast Iteration Settings")]
        public static void ApplyFastIterationSettingsMenu()
        {
            ApplyFastIterationSettings();
            EditorUtility.DisplayDialog(
                "DeepStake Android",
                "Fast Android iteration settings applied. Use DeepStake/Android/Fast Build And Run APK for phone smoke tests.",
                "OK");
        }

        [MenuItem("DeepStake/Android/Fast Build APK")]
        public static void BuildAndroidFast()
        {
            BuildAndroidFastInternal(false);
        }

        [MenuItem("DeepStake/Android/Fast Build And Run APK")]
        public static void BuildAndroidFastAndRun()
        {
            BuildAndroidFastInternal(true);
        }

        private static void BuildAndroidFastInternal(bool runAfterBuild)
        {
            ApplyFastIterationSettings();

            var buildRoot = Path.GetFullPath(Path.Combine("Builds", "Android"));
            Directory.CreateDirectory(buildRoot);
            var outputPath = Path.Combine(buildRoot, "DeepStake-fast.apk");

            var options = BuildOptions.CompressWithLz4;
            if (runAfterBuild)
            {
                options |= BuildOptions.AutoRunPlayer;
            }

            var buildOptions = new BuildPlayerOptions
            {
                scenes = ScenePaths,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = options
            };

            var report = BuildPipeline.BuildPlayer(buildOptions);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException("DeepStake fast Android build failed: " + report.summary.result);
            }
        }

        private static void ApplyFastIterationSettings()
        {
            ApplyAndroidIdentity();
            SwitchToAndroidIfNeeded();
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.development = false;
            EditorUserBuildSettings.connectProfiler = false;
            EditorUserBuildSettings.allowDebugging = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            TrySetAndroidCreateSymbolsDisabled();
        }

        private static void ApplyAndroidIdentity()
        {
            PlayerSettings.productName = "Deep Stake";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.roboheart.deepstake");
        }

        private static void SwitchToAndroidIfNeeded()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            }
        }

        private static void SwitchToWindowsIfNeeded()
        {
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            }
        }

        private static void ValidateScenes()
        {
            for (var index = 0; index < ScenePaths.Length; index++)
            {
                if (!File.Exists(ScenePaths[index]))
                {
                    throw new BuildFailedException("Required scene is missing: " + ScenePaths[index]);
                }
            }
        }

        private static void TrySetAndroidCreateSymbolsDisabled()
        {
            var androidType = typeof(PlayerSettings).GetNestedType("Android", BindingFlags.Public);
            var property = androidType?.GetProperty("androidCreateSymbols", BindingFlags.Public | BindingFlags.Static);
            if (property == null || !property.CanWrite || !property.PropertyType.IsEnum)
            {
                return;
            }

            try
            {
                var disabledValue = Enum.Parse(property.PropertyType, "Disabled");
                property.SetValue(null, disabledValue);
            }
            catch (Exception exception)
            {
                UnityEngine.Debug.LogWarning("Could not disable Android symbols automatically. " + exception.Message);
            }
        }
    }
}
