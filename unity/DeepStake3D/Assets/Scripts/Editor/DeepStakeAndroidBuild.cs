using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace DeepStake.EditorTools
{
    public static class DeepStakeAndroidBuild
    {
        private static readonly string[] ScenePaths =
        {
            "Assets/Scenes/Boot.unity",
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/WorldPrototype3D.unity",
        };

        public static void BuildAndroidDebug()
        {
            var buildRoot = Path.GetFullPath(Path.Combine("Builds", "Android"));
            Directory.CreateDirectory(buildRoot);
            var outputPath = Path.Combine(buildRoot, "DeepStake.apk");

            PlayerSettings.productName = "Deep Stake";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.roboheart.deepstake");
            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.development = true;
            EditorUserBuildSettings.connectProfiler = false;
            EditorUserBuildSettings.allowDebugging = true;

            var options = new BuildPlayerOptions
            {
                scenes = ScenePaths,
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            var report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException("DeepStake Android build failed: " + report.summary.result);
            }
        }
    }
}
