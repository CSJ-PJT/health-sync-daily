using System;
using UnityEngine;

namespace DeepStake.Boot
{
    public static class DeepStakeDevLaunchOptions
    {
        private const string AutorunArg = "-deepstakeAutorunLocalPlay";
        private const string ForceMobileControlsArg = "-deepstakeForceMobileControlsInEditor";
        private const string TagArgPrefix = "-deepstakeVerificationTag=";
        private const string AndroidAutorunExtra = "deepstake_dev_autorun";
        private const string AndroidTagExtra = "deepstake_verification_tag";

        private static bool initialized;
        private static bool autorunLocalPlay;
        private static bool forceMobileControlsInEditor;
        private static string verificationTag = string.Empty;

#if UNITY_EDITOR
        private static bool editorAutorunOverride;
        private static bool editorForceMobileControlsOverride;
        private static string editorVerificationTagOverride = string.Empty;
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

        private static void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            autorunLocalPlay = HasCommandLineFlag(AutorunArg);
            forceMobileControlsInEditor = HasCommandLineFlag(ForceMobileControlsArg);
            verificationTag = GetCommandLineValue(TagArgPrefix);

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

            if (!string.IsNullOrWhiteSpace(editorVerificationTagOverride))
            {
                verificationTag = editorVerificationTagOverride;
            }
#endif

            var allowDevMode = Debug.isDebugBuild;

#if UNITY_EDITOR
            allowDevMode |= editorAutorunOverride || editorForceMobileControlsOverride;
#endif

            if (!allowDevMode)
            {
                autorunLocalPlay = false;
                forceMobileControlsInEditor = false;
                verificationTag = string.Empty;
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

        public static void ClearEditorOverrides()
        {
            editorAutorunOverride = false;
            editorForceMobileControlsOverride = false;
            editorVerificationTagOverride = string.Empty;
            initialized = false;
        }
#endif
    }
}
