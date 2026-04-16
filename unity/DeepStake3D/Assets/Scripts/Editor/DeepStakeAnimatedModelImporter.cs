using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace DeepStake.Editor
{
    [InitializeOnLoad]
    internal static class DeepStakeAnimatedModelImporter
    {
        private const string AnimatedRoot = "Assets/Resources/AnimatedModels";
        private static bool processing;

        static DeepStakeAnimatedModelImporter()
        {
            EditorApplication.delayCall += () => EnsureAllAnimatedModels();
        }

        [MenuItem("DeepStake/Regenerate Animated Model Controllers")]
        private static void RegenerateAnimatedModelControllersMenu()
        {
            EnsureAllAnimatedModels(configureImporter: false, forceReimport: false);
        }

        [MenuItem("DeepStake/Reimport Animated Models")]
        private static void ReimportAnimatedModelsMenu()
        {
            EnsureAllAnimatedModels(configureImporter: true, forceReimport: true);
        }

        internal static bool IsAnimatedModelPath(string assetPath)
        {
            return !string.IsNullOrWhiteSpace(assetPath)
                   && assetPath.Replace('\\', '/').StartsWith(AnimatedRoot)
                   && assetPath.EndsWith(".fbx");
        }

        internal static void EnsureAllAnimatedModels(bool configureImporter = true, bool forceReimport = false)
        {
            if (processing)
            {
                return;
            }

            processing = true;
            try
            {
                var guids = AssetDatabase.FindAssets("t:Model", new[] { AnimatedRoot });
                for (var i = 0; i < guids.Length; i++)
                {
                    EditorUtility.DisplayProgressBar(
                        "DeepStake Animated Models",
                        $"Processing model {i + 1} / {guids.Length}",
                        guids.Length == 0 ? 1f : (float)i / guids.Length);

                    var assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (IsAnimatedModelPath(assetPath))
                    {
                        EnsureAnimatedModel(assetPath, configureImporter, forceReimport);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                processing = false;
            }
        }

        internal static void EnsureAnimatedModel(string assetPath, bool configureImporter = true, bool forceReimport = false)
        {
            if (!IsAnimatedModelPath(assetPath))
            {
                return;
            }

            var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            if (importer == null)
            {
                return;
            }

            var changed = false;
            if (configureImporter)
            {
                if (!importer.importAnimation)
                {
                    importer.importAnimation = true;
                    changed = true;
                }

                if (importer.animationType != ModelImporterAnimationType.Generic)
                {
                    importer.animationType = ModelImporterAnimationType.Generic;
                    changed = true;
                }

                if (changed || forceReimport)
                {
                    importer.SaveAndReimport();
                    importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                    if (importer == null)
                    {
                        return;
                    }
                }
            }

            if (configureImporter)
            {
                var defaultClips = importer.defaultClipAnimations;
                if (defaultClips != null && defaultClips.Length > 0)
                {
                    var existing = importer.clipAnimations;
                    var needsClipSetup = existing == null || existing.Length != defaultClips.Length;
                    if (!needsClipSetup)
                    {
                        for (var i = 0; i < existing.Length; i++)
                        {
                            if (existing[i].name != defaultClips[i].name)
                            {
                                needsClipSetup = true;
                                break;
                            }
                        }
                    }

                    if (needsClipSetup)
                    {
                        var configured = new ModelImporterClipAnimation[defaultClips.Length];
                        for (var i = 0; i < defaultClips.Length; i++)
                        {
                            var clip = defaultClips[i];
                            var lower = (clip.name ?? string.Empty).ToLowerInvariant();
                            var loop = lower.Contains("idle") || lower.Contains("walk") || lower.Contains("run") || lower.Contains("move");
                            clip.loopTime = loop;
                            clip.loopPose = loop;
                            configured[i] = clip;
                        }

                        importer.clipAnimations = configured;
                        importer.SaveAndReimport();
                        return;
                    }
                }
            }

            CreateAnimatorController(assetPath);
        }

        private static void CreateAnimatorController(string assetPath)
        {
            var clips = AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<AnimationClip>()
                .Where(clip => clip != null && !clip.name.StartsWith("__preview__", System.StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (clips.Length == 0)
            {
                return;
            }

            var controllerPath = assetPath.Replace(".fbx", "_Auto.controller");
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            if (existing != null)
            {
                AssetDatabase.DeleteAsset(controllerPath);
            }

            var controller = AnimatorController.CreateAnimatorControllerAtPath(controllerPath);
            var stateMachine = controller.layers[0].stateMachine;
            AnimatorState defaultState = null;

            for (var i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                var state = stateMachine.AddState(SanitizeStateName(clip.name, i));
                state.motion = clip;
                if (defaultState == null || clip.name.ToLowerInvariant().Contains("idle"))
                {
                    defaultState = state;
                }
            }

            if (defaultState == null && stateMachine.states.Length > 0)
            {
                defaultState = stateMachine.states[0].state;
            }

            stateMachine.defaultState = defaultState;
            AssetDatabase.SaveAssets();
        }

        private static string SanitizeStateName(string rawName, int index)
        {
            var sanitized = string.IsNullOrWhiteSpace(rawName)
                ? $"Clip_{index}"
                : Regex.Replace(rawName, @"[^A-Za-z0-9_\- ]", "_");

            sanitized = Regex.Replace(sanitized, @"\s+", " ").Trim();
            if (string.IsNullOrWhiteSpace(sanitized))
            {
                sanitized = $"Clip_{index}";
            }

            return sanitized;
        }
    }

    internal sealed class DeepStakeAnimatedModelPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            var shouldProcess = importedAssets.Any(DeepStakeAnimatedModelImporter.IsAnimatedModelPath)
                                || movedAssets.Any(DeepStakeAnimatedModelImporter.IsAnimatedModelPath);

            if (!shouldProcess)
            {
                return;
            }

            EditorApplication.delayCall += () => DeepStakeAnimatedModelImporter.EnsureAllAnimatedModels(configureImporter: true, forceReimport: false);
        }
    }
}
