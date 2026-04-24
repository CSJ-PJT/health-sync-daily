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
        internal const string IdleStateName = "DS_Idle";
        internal const string WalkStateName = "DS_Walk";
        internal const string RunStateName = "DS_Run";
        internal const string TalkStateName = "DS_Talk";
        internal const string PlaceStateName = "DS_Place";
        internal const string AttackStateName = "DS_Attack";
        internal const string HitStateName = "DS_Hit";
        internal const string DeathStateName = "DS_Death";
        private static bool processing;

        static DeepStakeAnimatedModelImporter()
        {
            if (Application.isBatchMode)
            {
                Debug.Log("[DeepStakeBuild] Batch mode detected. Skipping animated model auto-ensure.");
                return;
            }

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
            var semanticClips = BuildSemanticClipMap(clips);
            AnimatorState defaultState = null;

            defaultState = AddStateIfClipExists(stateMachine, IdleStateName, semanticClips.idle, defaultState);
            defaultState = AddStateIfClipExists(stateMachine, WalkStateName, semanticClips.walk, defaultState);
            AddStateIfClipExists(stateMachine, RunStateName, semanticClips.run, defaultState);
            AddStateIfClipExists(stateMachine, TalkStateName, semanticClips.talk, defaultState);
            AddStateIfClipExists(stateMachine, PlaceStateName, semanticClips.place, defaultState);
            AddStateIfClipExists(stateMachine, AttackStateName, semanticClips.attack, defaultState);
            AddStateIfClipExists(stateMachine, HitStateName, semanticClips.hit, defaultState);
            AddStateIfClipExists(stateMachine, DeathStateName, semanticClips.death, defaultState);

            if (stateMachine.states.Length == 0)
            {
                for (var i = 0; i < clips.Length; i++)
                {
                    var state = stateMachine.AddState(SanitizeStateName(clips[i].name, i));
                    state.motion = clips[i];
                    if (defaultState == null)
                    {
                        defaultState = state;
                    }
                }
            }

            if (defaultState == null && stateMachine.states.Length > 0)
            {
                defaultState = stateMachine.states[0].state;
            }

            stateMachine.defaultState = defaultState;
            AssetDatabase.SaveAssets();
        }

        private static AnimatorState AddStateIfClipExists(AnimatorStateMachine stateMachine, string stateName, AnimationClip clip, AnimatorState defaultState)
        {
            if (clip == null)
            {
                return defaultState;
            }

            var state = stateMachine.AddState(stateName);
            state.motion = clip;
            return defaultState ?? state;
        }

        private static (AnimationClip idle, AnimationClip walk, AnimationClip run, AnimationClip talk, AnimationClip place, AnimationClip attack, AnimationClip hit, AnimationClip death) BuildSemanticClipMap(AnimationClip[] clips)
        {
            AnimationClip idle = null;
            AnimationClip walk = null;
            AnimationClip run = null;
            AnimationClip talk = null;
            AnimationClip place = null;
            AnimationClip attack = null;
            AnimationClip hit = null;
            AnimationClip death = null;

            for (var i = 0; i < clips.Length; i++)
            {
                var clip = clips[i];
                var key = clip.name.ToLowerInvariant();

                if (idle == null && (key.Contains("idle") || key.Contains("taunt")))
                {
                    idle = clip;
                    continue;
                }

                if (run == null && (key.Contains("run") || key.Contains("running") || key.Contains("sprint") || key.Contains("dash")))
                {
                    run = clip;
                    continue;
                }

                if (walk == null && (key.Contains("walk") || key.Contains("walking") || key.Contains("move")))
                {
                    walk = clip;
                    continue;
                }

                if (death == null && (key.Contains("dead") || key.Contains("death") || key.Contains("knock_down") || key.Contains("knockdown")))
                {
                    death = clip;
                    continue;
                }

                if (hit == null && (key.Contains("reaction") || key.Contains("behit") || key.Contains("hit") || key.Contains("electrocution") || key.Contains("slap_reaction")))
                {
                    hit = clip;
                    continue;
                }

                if (attack == null && (key.Contains("attack") || key.Contains("slash") || key.Contains("punch") || key.Contains("hook") || key.Contains("jab") || key.Contains("uppercut") || key.Contains("kick") || key.Contains("combo") || key.Contains("shot") || key.Contains("spartan")))
                {
                    attack = clip;
                    continue;
                }

                if (place == null && (key.Contains("cast") || key.Contains("vault") || key.Contains("aim") || key.Contains("draw_and_shoot") || key.Contains("use")))
                {
                    place = clip;
                    continue;
                }

                if (talk == null && (key.Contains("talk") || key.Contains("taunt") || key.Contains("aim")))
                {
                    talk = clip;
                }
            }

            run ??= clips.FirstOrDefault(clip => clip.name.ToLowerInvariant().Contains("run"))
                    ?? clips.FirstOrDefault(clip => clip.name.ToLowerInvariant().Contains("running"));
            walk ??= clips.FirstOrDefault(clip => clip.name.ToLowerInvariant().Contains("walk"))
                     ?? run
                     ?? clips.FirstOrDefault();
            idle ??= clips.FirstOrDefault(clip => clip.name.ToLowerInvariant().Contains("idle"))
                     ?? walk;
            talk ??= idle;
            place ??= clips.FirstOrDefault(clip => clip.name.ToLowerInvariant().Contains("cast"))
                      ?? attack
                      ?? talk;
            attack ??= clips.FirstOrDefault(clip => clip.name.ToLowerInvariant().Contains("attack"))
                       ?? clips.FirstOrDefault(clip => clip.name.ToLowerInvariant().Contains("combo"))
                       ?? walk;
            hit ??= clips.FirstOrDefault(clip => clip.name.ToLowerInvariant().Contains("reaction"))
                    ?? clips.FirstOrDefault(clip => clip.name.ToLowerInvariant().Contains("hit"))
                    ?? attack;
            death ??= clips.FirstOrDefault(clip => clip.name.ToLowerInvariant().Contains("dead"))
                      ?? clips.FirstOrDefault(clip => clip.name.ToLowerInvariant().Contains("knock"))
                      ?? hit;

            return (idle, walk, run, talk, place, attack, hit, death);
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
            if (Application.isBatchMode)
            {
                return;
            }

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
