using System;
using UnityEngine;

namespace DeepStake.Boot
{
    public static class DeepStakeDevLaunchOptions
    {
        private const string AutorunArg = "-deepstakeAutorunLocalPlay";
        private const string ForceMobileControlsArg = "-deepstakeForceMobileControlsInEditor";
        private const string CaptureScreenshotArg = "-deepstakeCaptureScreenshot";
        private const string QuitAfterScreenshotArg = "-deepstakeQuitAfterScreenshot";
        private const string TagArgPrefix = "-deepstakeVerificationTag=";
        private const string ScreenshotDirArgPrefix = "-deepstakeScreenshotDir=";
        private const string AndroidAutorunExtra = "deepstake_dev_autorun";
        private const string AndroidTagExtra = "deepstake_verification_tag";

        private static bool initialized;
        private static bool autorunLocalPlay;
        private static bool forceMobileControlsInEditor;
        private static bool captureLocalScreenshot;
        private static bool quitAfterScreenshot;
        private static string verificationTag = string.Empty;
        private static string screenshotDirectory = string.Empty;

#if UNITY_EDITOR
        private static bool editorAutorunOverride;
        private static bool editorForceMobileControlsOverride;
        private static bool editorCaptureScreenshotOverride;
        private static bool editorQuitAfterScreenshotOverride;
        private static string editorVerificationTagOverride = string.Empty;
        private static string editorScreenshotDirectoryOverride = string.Empty;
#endif

        public static bool AutorunLocalPlay
        {
            get
            {
                EnsureInitialized();
                return autorunLocalPlay;
            }
        }

        public static bool ForceMobileControlsInEditor
        {
            get
            {
                EnsureInitialized();
                return forceMobileControlsInEditor;
            }
        }

        public static string VerificationTag
        {
            get
            {
                EnsureInitialized();
                return verificationTag;
            }
        }

        public static bool CaptureLocalScreenshot
        {
            get
            {
                EnsureInitialized();
                return captureLocalScreenshot;
            }
        }

        public static bool QuitAfterScreenshot
        {
            get
            {
                EnsureInitialized();
                return quitAfterScreenshot;
            }
        }

        public static string ScreenshotDirectory
        {
            get
            {
                EnsureInitialized();
                return screenshotDirectory;
            }
        }

        private static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            autorunLocalPlay = HasCommandLineFlag(AutorunArg);
            forceMobileControlsInEditor = HasCommandLineFlag(ForceMobileControlsArg);
            captureLocalScreenshot = HasCommandLineFlag(CaptureScreenshotArg);
            quitAfterScreenshot = HasCommandLineFlag(QuitAfterScreenshotArg);
            verificationTag = GetCommandLineValue(TagArgPrefix);
            screenshotDirectory = GetCommandLineValue(ScreenshotDirArgPrefix);

#if UNITY_ANDROID && !UNITY_EDITOR
            TryReadAndroidIntentExtras(ref autorunLocalPlay, ref verificationTag);
#endif

#if UNITY_EDITOR
            if (editorAutorunOverride)
            {
                autorunLocalPlay = true;
            }

            if (editorForceMobileControlsOverride)
            {
                forceMobileControlsInEditor = true;
            }

            if (editorCaptureScreenshotOverride)
            {
                captureLocalScreenshot = true;
            }

            if (editorQuitAfterScreenshotOverride)
            {
                quitAfterScreenshot = true;
            }

            if (!string.IsNullOrWhiteSpace(editorVerificationTagOverride))
            {
                verificationTag = editorVerificationTagOverride;
            }

            if (!string.IsNullOrWhiteSpace(editorScreenshotDirectoryOverride))
            {
                screenshotDirectory = editorScreenshotDirectoryOverride;
            }
#endif

            var allowDevMode = Debug.isDebugBuild;

#if UNITY_EDITOR
            allowDevMode |= editorAutorunOverride || editorForceMobileControlsOverride || editorCaptureScreenshotOverride || editorQuitAfterScreenshotOverride;
#endif

            if (!allowDevMode)
            {
                autorunLocalPlay = false;
                forceMobileControlsInEditor = false;
                captureLocalScreenshot = false;
                quitAfterScreenshot = false;
                verificationTag = string.Empty;
                screenshotDirectory = string.Empty;
                return;
            }

            if (!autorunLocalPlay && Application.platform == RuntimePlatform.Android)
            {
                autorunLocalPlay = true;
                if (string.IsNullOrWhiteSpace(verificationTag))
                {
                    verificationTag = "android-debug-default";
                }
            }

            if (autorunLocalPlay && string.IsNullOrWhiteSpace(verificationTag))
            {
                verificationTag = Application.isEditor ? "editor-dev-default" : "untagged";
            }

            if (captureLocalScreenshot && string.IsNullOrWhiteSpace(screenshotDirectory))
            {
                screenshotDirectory = Application.isEditor ? "Pictures/Screenshot" : string.Empty;
            }

            if (autorunLocalPlay)
            {
                Debug.Log("[DeepStakeDev] Autorun local play requested. tag=" + VerificationTagOrDefault());
            }

            if (forceMobileControlsInEditor)
            {
                Debug.Log("[DeepStakeDev] Mobile controls forced for local/editor test mode.");
            }
        }

        private static bool HasCommandLineFlag(string flag)
        {
            var args = Environment.GetCommandLineArgs();
            for (var index = 0; index < args.Length; index++)
            {
                if (string.Equals(args[index], flag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string GetCommandLineValue(string prefix)
        {
            var args = Environment.GetCommandLineArgs();
            for (var index = 0; index < args.Length; index++)
            {
                var value = args[index];
                if (value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return value.Substring(prefix.Length);
                }
            }

            return string.Empty;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private static void TryReadAndroidIntentExtras(ref bool autorun, ref string tag)
        {
            try
            {
                using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                using var intent = activity.Call<AndroidJavaObject>("getIntent");
                if (intent == null)
                {
                    return;
                }

                if (intent.Call<bool>("hasExtra", AndroidAutorunExtra))
                {
                    autorun = intent.Call<bool>("getBooleanExtra", AndroidAutorunExtra, autorun);
                }

                if (intent.Call<bool>("hasExtra", AndroidTagExtra))
                {
                    tag = intent.Call<string>("getStringExtra", AndroidTagExtra);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[DeepStakeDev] Could not read Android intent extras. " + exception.Message);
            }
        }
#endif

        private static string VerificationTagOrDefault()
        {
            return string.IsNullOrWhiteSpace(verificationTag) ? "untagged" : verificationTag;
        }

#if UNITY_EDITOR
        public static void SetEditorOverrides(bool autorun, bool forceMobileControls, string tag = "")
        {
            editorAutorunOverride = autorun;
            editorForceMobileControlsOverride = forceMobileControls;
            editorVerificationTagOverride = tag ?? string.Empty;
            initialized = false;
        }

        public static void SetEditorOverrides(bool autorun, bool forceMobileControls, string tag, bool captureScreenshot, string screenshotDirectoryOverride, bool quitAfterScreenshotOverride)
        {
            editorAutorunOverride = autorun;
            editorForceMobileControlsOverride = forceMobileControls;
            editorCaptureScreenshotOverride = captureScreenshot;
            editorQuitAfterScreenshotOverride = quitAfterScreenshotOverride;
            editorVerificationTagOverride = tag ?? string.Empty;
            editorScreenshotDirectoryOverride = screenshotDirectoryOverride ?? string.Empty;
            initialized = false;
        }

        public static void ClearEditorOverrides()
        {
            editorAutorunOverride = false;
            editorForceMobileControlsOverride = false;
            editorCaptureScreenshotOverride = false;
            editorQuitAfterScreenshotOverride = false;
            editorVerificationTagOverride = string.Empty;
            editorScreenshotDirectoryOverride = string.Empty;
            initialized = false;
        }
#endif
    }
}
