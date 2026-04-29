using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeepStake.Boot;
using DeepStake.CameraRig;
using DeepStake.Environment;
using DeepStake.Interaction;
using DeepStake.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DeepStake.EditorTools
{
    public static class DeepStakeMeshyEnvironmentPass
    {
        private const string WorldScenePath = "Assets/Scenes/WorldPrototype3D.unity";
        private const string RegistryPath = "Assets/Resources/Meshy/deepstake_meshy_model_registry.json";
        private const string PlacementMappingPath = "Assets/Resources/Meshy/deepstake_meshy_placement_mapping.json";
        private const string DressingRootName = "MeshyVisualDressing";
        private const string ValidationReportPath = "TestResults/meshy-scene-validation-report.txt";

        private static readonly string[] FirstPlacementAssetOrder =
        {
            "door_wood_old",
            "window_dark_frame",
            "tree_dry_small",
            "prop_notice_board",
            "prop_wood_fence",
            "prop_rusty_barrel",
            "prop_lamp_post",
            "prop_supply_crate"
        };

        public static void CreatePrefabsAndApplyFirstPlacementCli()
        {
            try
            {
                var registry = LoadRegistry();
                var placement = LoadPlacementMapping();
                EnsurePrefabAssets(registry);
                ApplyFirstControlledPlacement(registry, placement);
                SaveRegistry(registry);
                Debug.Log("[DeepStakeMeshy] Prefab generation and first controlled placement succeeded.");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError("[DeepStakeMeshy] First placement pass failed: " + exception);
                EditorApplication.Exit(1);
            }
        }

        public static void ValidateAppliedSceneCli()
        {
            try
            {
                var registry = LoadRegistry();
                var report = ValidateAppliedScene(registry);
                File.WriteAllText(ValidationReportPath, report);
                Debug.Log("[DeepStakeMeshy] Scene validation succeeded. report=" + ValidationReportPath);
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogError("[DeepStakeMeshy] Scene validation failed: " + exception);
                EditorApplication.Exit(1);
            }
        }

        private static DeepStakeMeshyModelRegistry LoadRegistry()
        {
            var json = File.ReadAllText(RegistryPath);
            var registry = JsonUtility.FromJson<DeepStakeMeshyModelRegistry>(json);
            if (registry == null)
            {
                throw new InvalidOperationException("Unable to load Meshy registry.");
            }

            return registry;
        }

        private static DeepStakeMeshyPlacementMapping LoadPlacementMapping()
        {
            var json = File.ReadAllText(PlacementMappingPath);
            var mapping = JsonUtility.FromJson<DeepStakeMeshyPlacementMapping>(json);
            if (mapping == null)
            {
                throw new InvalidOperationException("Unable to load Meshy placement mapping.");
            }

            return mapping;
        }

        private static void SaveRegistry(DeepStakeMeshyModelRegistry registry)
        {
            var json = JsonUtility.ToJson(registry, true);
            File.WriteAllText(RegistryPath, json);
            AssetDatabase.ImportAsset(RegistryPath);
        }

        private static void EnsurePrefabAssets(DeepStakeMeshyModelRegistry registry)
        {
            foreach (var entry in registry.entries)
            {
                if (!string.Equals(entry.status, "curated_model_ready", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(entry.status, "prefab_ready", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(entry.status, "applied", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var prefabAsset = AssetDatabase.LoadAssetAtPath<GameObject>(entry.prefabPath);
                if (prefabAsset != null)
                {
                    if (!string.Equals(entry.status, "applied", StringComparison.OrdinalIgnoreCase))
                    {
                        entry.status = "prefab_ready";
                    }
                    continue;
                }

                var curatedFolder = entry.curatedModelPath.TrimEnd('/', '\\');
                if (!AssetDatabase.IsValidFolder(curatedFolder))
                {
                    continue;
                }

                var primaryModelPath = FindPrimaryModelAsset(curatedFolder);
                if (string.IsNullOrWhiteSpace(primaryModelPath))
                {
                    continue;
                }

                var importedModel = AssetDatabase.LoadAssetAtPath<GameObject>(primaryModelPath);
                if (importedModel == null)
                {
                    continue;
                }

                var prefabDirectory = Path.GetDirectoryName(entry.prefabPath)?.Replace('\\', '/');
                EnsureAssetFolder(prefabDirectory);

                var instance = PrefabUtility.InstantiatePrefab(importedModel) as GameObject;
                if (instance == null)
                {
                    continue;
                }

                try
                {
                    instance.name = entry.assetId;
                    PrefabUtility.SaveAsPrefabAsset(instance, entry.prefabPath);
                    entry.status = "prefab_ready";
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(instance);
                }
            }
        }

        private static void ApplyFirstControlledPlacement(
            DeepStakeMeshyModelRegistry registry,
            DeepStakeMeshyPlacementMapping placementMapping)
        {
            EditorSceneManager.OpenScene(WorldScenePath, OpenSceneMode.Single);
            var controller = UnityEngine.Object.FindFirstObjectByType<WorldPrototype3DController>();
            if (controller == null)
            {
                throw new InvalidOperationException("WorldPrototype3DController not found.");
            }

            var serializedController = new SerializedObject(controller);
            var zoneRoot = GetObjectReference<Transform>(serializedController, "zoneRoot");
            var playerTransform = GetObjectReference<Transform>(serializedController, "playerTransform");
            var worldPrototypeJson = GetObjectReference<TextAsset>(serializedController, "worldPrototypeJson");
            var definition = WorldPrototype3DDefinition.FromJson(worldPrototypeJson);

            if (zoneRoot == null)
            {
                throw new InvalidOperationException("zoneRoot is not assigned.");
            }

            WorldPrototypeVisualPass.RebuildZoneVisuals(
                zoneRoot,
                definition,
                GetObjectReference<Material>(serializedController, "fieldMaterial"),
                GetObjectReference<Material>(serializedController, "archiveMaterial"),
                GetObjectReference<Material>(serializedController, "placementMaterial"),
                GetObjectReference<Material>(serializedController, "roadMaterial"),
                GetObjectReference<Material>(serializedController, "storageMaterial"));

            var dressingRoot = GameObject.Find(DressingRootName);
            if (dressingRoot == null)
            {
                dressingRoot = new GameObject(DressingRootName);
            }

            ClearChildren(dressingRoot.transform);

            var prefabLookup = registry.entries
                .Where(entry => string.Equals(entry.status, "prefab_ready", StringComparison.OrdinalIgnoreCase) ||
                                string.Equals(entry.status, "applied", StringComparison.OrdinalIgnoreCase))
                .ToDictionary(entry => entry.assetId, entry => AssetDatabase.LoadAssetAtPath<GameObject>(entry.prefabPath));

            var appliedAssetIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            PlaceDoorPrefab(zoneRoot, dressingRoot.transform, prefabLookup, appliedAssetIds, "door_wood_old");
            PlaceWindowPrefabs(zoneRoot, dressingRoot.transform, prefabLookup, appliedAssetIds, "window_dark_frame");
            PlaceNoticeBoard(controller, dressingRoot.transform, prefabLookup, appliedAssetIds, "prop_notice_board");
            PlaceFenceSegments(zoneRoot, dressingRoot.transform, prefabLookup, appliedAssetIds, "prop_wood_fence");
            PlaceBarrels(zoneRoot, dressingRoot.transform, prefabLookup, appliedAssetIds, "prop_rusty_barrel");
            PlaceLampPost(zoneRoot, dressingRoot.transform, prefabLookup, appliedAssetIds, "prop_lamp_post");
            PlaceSupplyCrate(controller, dressingRoot.transform, prefabLookup, appliedAssetIds, "prop_supply_crate");
            PlaceDryTrees(zoneRoot, playerTransform, dressingRoot.transform, prefabLookup, appliedAssetIds, "tree_dry_small");

            foreach (var entry in registry.entries)
            {
                if (!appliedAssetIds.Contains(entry.assetId))
                {
                    continue;
                }

                entry.status = "applied";
            }

            ClearChildren(zoneRoot);
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        private static string ValidateAppliedScene(DeepStakeMeshyModelRegistry registry)
        {
            EditorSceneManager.OpenScene(WorldScenePath, OpenSceneMode.Single);
            var builder = new StringBuilder();
            builder.AppendLine("scene_opened=true");
            builder.AppendLine("scene_path=" + WorldScenePath);
            builder.AppendLine("compile_status=passed_batch_open_check");

            var dressingRoot = GameObject.Find(DressingRootName);
            builder.AppendLine("dressing_root_found=" + (dressingRoot != null).ToString().ToLowerInvariant());
            builder.AppendLine("dressing_root_name=" + DressingRootName);

            var appliedEntries = registry.entries
                .Where(entry => string.Equals(entry.status, "applied", StringComparison.OrdinalIgnoreCase))
                .ToArray();
            var prefabReadyEntries = registry.entries
                .Where(entry => string.Equals(entry.status, "prefab_ready", StringComparison.OrdinalIgnoreCase))
                .ToArray();

            builder.AppendLine("applied_registry_count=" + appliedEntries.Length);
            builder.AppendLine("prefab_ready_registry_count=" + prefabReadyEntries.Length);
            builder.AppendLine("applied_registry_entries=" + string.Join(",", appliedEntries.Select(entry => entry.assetId)));
            builder.AppendLine("prefab_ready_registry_entries=" + string.Join(",", prefabReadyEntries.Select(entry => entry.assetId)));

            var missingPrefabs = new List<string>();
            foreach (var entry in registry.entries.Where(entry =>
                         string.Equals(entry.status, "applied", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(entry.status, "prefab_ready", StringComparison.OrdinalIgnoreCase)))
            {
                if (AssetDatabase.LoadAssetAtPath<GameObject>(entry.prefabPath) == null)
                {
                    missingPrefabs.Add(entry.assetId + ":" + entry.prefabPath);
                }
            }

            builder.AppendLine("missing_prefab_count=" + missingPrefabs.Count);
            builder.AppendLine("missing_prefabs=" + string.Join(";", missingPrefabs));

            if (dressingRoot != null)
            {
                var groupNames = new List<string>();
                for (var index = 0; index < dressingRoot.transform.childCount; index++)
                {
                    var child = dressingRoot.transform.GetChild(index);
                    groupNames.Add(child.name + "(" + child.childCount + ")");
                }

                builder.AppendLine("placed_group_count=" + dressingRoot.transform.childCount);
                builder.AppendLine("placed_groups=" + string.Join(",", groupNames));
            }
            else
            {
                builder.AppendLine("placed_group_count=0");
                builder.AppendLine("placed_groups=");
            }

            return builder.ToString();
        }

        private static void PlaceDoorPrefab(
            Transform zoneRoot,
            Transform dressingRoot,
            IReadOnlyDictionary<string, GameObject> prefabLookup,
            ISet<string> appliedAssetIds,
            string assetId)
        {
            if (!prefabLookup.TryGetValue(assetId, out var prefab) || prefab == null)
            {
                return;
            }

            var targets = new[]
            {
                FindChildByName(zoneRoot, "ArchiveBuilding_DoorwayShadow"),
                FindChildByName(zoneRoot, "ClinicCottage_DoorwayShadow")
            }.Where(target => target != null);

            var group = EnsureGroup(dressingRoot, assetId);
            foreach (var target in targets)
            {
                PlaceFittedPrefab(group, prefab, target, 180f, new Vector3(0f, -0.42f, -0.03f), FitAxis.WidthAndHeight);
                appliedAssetIds.Add(assetId);
            }
        }

        private static void PlaceWindowPrefabs(
            Transform zoneRoot,
            Transform dressingRoot,
            IReadOnlyDictionary<string, GameObject> prefabLookup,
            ISet<string> appliedAssetIds,
            string assetId)
        {
            if (!prefabLookup.TryGetValue(assetId, out var prefab) || prefab == null)
            {
                return;
            }

            var targetNames = new[]
            {
                "ArchiveBuilding_WindowL_Glass",
                "ArchiveBuilding_WindowR_Glass",
                "ClinicCottage_WindowL_Glass"
            };

            var group = EnsureGroup(dressingRoot, assetId);
            foreach (var name in targetNames)
            {
                var target = FindChildByName(zoneRoot, name);
                if (target == null)
                {
                    continue;
                }

                PlaceFittedPrefab(group, prefab, target, 180f, new Vector3(0f, 0f, -0.03f), FitAxis.WidthAndHeight);
                appliedAssetIds.Add(assetId);
            }
        }

        private static void PlaceNoticeBoard(
            WorldPrototype3DController controller,
            Transform dressingRoot,
            IReadOnlyDictionary<string, GameObject> prefabLookup,
            ISet<string> appliedAssetIds,
            string assetId)
        {
            if (!prefabLookup.TryGetValue(assetId, out var prefab) || prefab == null || controller.PrimaryInteractable == null)
            {
                return;
            }

            var anchor = controller.PrimaryInteractable.transform;
            var group = EnsureGroup(dressingRoot, assetId);
            PlaceFittedPrefab(group, prefab, anchor, -90f, new Vector3(0f, -0.72f, 0f), FitAxis.HeightOnly, 1.35f);
            appliedAssetIds.Add(assetId);
        }

        private static void PlaceFenceSegments(
            Transform zoneRoot,
            Transform dressingRoot,
            IReadOnlyDictionary<string, GameObject> prefabLookup,
            ISet<string> appliedAssetIds,
            string assetId)
        {
            if (!prefabLookup.TryGetValue(assetId, out var prefab) || prefab == null)
            {
                return;
            }

            var segments = new[]
            {
                ("ArchiveYardFence_Post_1", "ArchiveYardFence_Post_2"),
                ("FieldLowFence_Post_1", "FieldLowFence_Post_2"),
                ("NorthFence_Post_2", "NorthFence_Post_3")
            };

            var group = EnsureGroup(dressingRoot, assetId);
            foreach (var pair in segments)
            {
                var from = FindChildByName(zoneRoot, pair.Item1);
                var to = FindChildByName(zoneRoot, pair.Item2);
                if (from == null || to == null)
                {
                    continue;
                }

                var midpoint = (from.position + to.position) * 0.5f;
                var rotation = Quaternion.LookRotation((to.position - from.position).normalized, Vector3.up);
                var instance = PrefabUtility.InstantiatePrefab(prefab, group) as GameObject;
                if (instance == null)
                {
                    continue;
                }

                instance.transform.position = midpoint;
                instance.transform.rotation = rotation;
                FitInstanceToTargetDistance(instance, Vector3.Distance(from.position, to.position), 0.9f);
                appliedAssetIds.Add(assetId);
            }
        }

        private static void PlaceBarrels(
            Transform zoneRoot,
            Transform dressingRoot,
            IReadOnlyDictionary<string, GameObject> prefabLookup,
            ISet<string> appliedAssetIds,
            string assetId)
        {
            if (!prefabLookup.TryGetValue(assetId, out var prefab) || prefab == null)
            {
                return;
            }

            var targetNames = new[]
            {
                "WestWorkshopBarrels_A",
                "WestWorkshopBarrels_B"
            };

            var group = EnsureGroup(dressingRoot, assetId);
            foreach (var name in targetNames)
            {
                var target = FindChildByName(zoneRoot, name);
                if (target == null)
                {
                    continue;
                }

                PlaceFittedPrefab(group, prefab, target, 0f, new Vector3(0f, -0.18f, 0f), FitAxis.HeightOnly, 1.15f);
                appliedAssetIds.Add(assetId);
            }
        }

        private static void PlaceLampPost(
            Transform zoneRoot,
            Transform dressingRoot,
            IReadOnlyDictionary<string, GameObject> prefabLookup,
            ISet<string> appliedAssetIds,
            string assetId)
        {
            if (!prefabLookup.TryGetValue(assetId, out var prefab) || prefab == null)
            {
                return;
            }

            var target = FindChildByName(zoneRoot, "ArchiveGuideClean_Post");
            if (target == null)
            {
                return;
            }

            var group = EnsureGroup(dressingRoot, assetId);
            PlaceFittedPrefab(group, prefab, target, 0f, new Vector3(0f, -0.32f, 0f), FitAxis.HeightOnly, 1.45f);
            appliedAssetIds.Add(assetId);
        }

        private static void PlaceSupplyCrate(
            WorldPrototype3DController controller,
            Transform dressingRoot,
            IReadOnlyDictionary<string, GameObject> prefabLookup,
            ISet<string> appliedAssetIds,
            string assetId)
        {
            if (!prefabLookup.TryGetValue(assetId, out var prefab) || prefab == null || controller.SecondaryInteractable == null)
            {
                return;
            }

            var anchor = controller.SecondaryInteractable.transform;
            var group = EnsureGroup(dressingRoot, assetId);
            PlaceFittedPrefab(group, prefab, anchor, 0f, new Vector3(0.18f, -0.58f, 0.12f), FitAxis.HeightOnly, 0.95f);
            appliedAssetIds.Add(assetId);
        }

        private static void PlaceDryTrees(
            Transform zoneRoot,
            Transform playerTransform,
            Transform dressingRoot,
            IReadOnlyDictionary<string, GameObject> prefabLookup,
            ISet<string> appliedAssetIds,
            string assetId)
        {
            if (!prefabLookup.TryGetValue(assetId, out var prefab) || prefab == null)
            {
                return;
            }

            var posts = new[]
            {
                FindChildByName(zoneRoot, "NorthFence_Post_0"),
                FindChildByName(zoneRoot, "NorthFence_Post_5")
            }.Where(target => target != null);

            var group = EnsureGroup(dressingRoot, assetId);
            foreach (var post in posts)
            {
                var instance = PrefabUtility.InstantiatePrefab(prefab, group) as GameObject;
                if (instance == null)
                {
                    continue;
                }

                instance.transform.position = post.position + new Vector3(0f, -0.45f, 0.85f);
                instance.transform.rotation = Quaternion.LookRotation((post.position - playerTransform.position).normalized, Vector3.up);
                FitInstanceByHeight(instance, 2.4f);
                appliedAssetIds.Add(assetId);
            }
        }

        private static void PlaceFittedPrefab(
            Transform parent,
            GameObject prefab,
            Transform target,
            float yawOffset,
            Vector3 positionOffset,
            FitAxis fitAxis,
            float heightMultiplier = 1f)
        {
            var instance = PrefabUtility.InstantiatePrefab(prefab, parent) as GameObject;
            if (instance == null)
            {
                return;
            }

            instance.transform.position = target.position + positionOffset;
            instance.transform.rotation = target.rotation * Quaternion.Euler(0f, yawOffset, 0f);

            switch (fitAxis)
            {
                case FitAxis.WidthAndHeight:
                    FitInstanceToTargetSize(instance, target, true, heightMultiplier);
                    break;
                case FitAxis.HeightOnly:
                    FitInstanceByHeight(instance, Mathf.Max(0.1f, target.lossyScale.y * heightMultiplier));
                    break;
            }
        }

        private static void FitInstanceToTargetSize(GameObject instance, Transform target, bool useWidth, float heightMultiplier)
        {
            if (!TryGetRendererBounds(instance, out var bounds))
            {
                return;
            }

            var width = useWidth ? Mathf.Max(0.1f, target.lossyScale.x) : bounds.size.x;
            var height = Mathf.Max(0.1f, target.lossyScale.y * heightMultiplier);
            var widthScale = width / Mathf.Max(0.001f, bounds.size.x);
            var heightScale = height / Mathf.Max(0.001f, bounds.size.y);
            var uniform = Mathf.Min(widthScale, heightScale);
            instance.transform.localScale = Vector3.one * uniform;
        }

        private static void FitInstanceByHeight(GameObject instance, float targetHeight)
        {
            if (!TryGetRendererBounds(instance, out var bounds))
            {
                return;
            }

            var uniform = targetHeight / Mathf.Max(0.001f, bounds.size.y);
            instance.transform.localScale = Vector3.one * uniform;
        }

        private static void FitInstanceToTargetDistance(GameObject instance, float targetDistance, float heightMultiplier)
        {
            if (!TryGetRendererBounds(instance, out var bounds))
            {
                return;
            }

            var lengthScale = targetDistance / Mathf.Max(0.001f, Mathf.Max(bounds.size.x, bounds.size.z));
            var heightScale = heightMultiplier / Mathf.Max(0.001f, bounds.size.y);
            var uniform = Mathf.Min(lengthScale, heightScale);
            instance.transform.localScale = Vector3.one * uniform;
        }

        private static bool TryGetRendererBounds(GameObject instance, out Bounds bounds)
        {
            var renderers = instance.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0)
            {
                bounds = default;
                return false;
            }

            bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++)
            {
                bounds.Encapsulate(renderers[index].bounds);
            }

            return true;
        }

        private static string FindPrimaryModelAsset(string curatedFolder)
        {
            var guids = AssetDatabase.FindAssets(string.Empty, new[] { curatedFolder });
            var candidates = new List<string>();
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (path.EndsWith(".glb", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                {
                    candidates.Add(path);
                }
            }

            return candidates
                .OrderBy(path => path.EndsWith(".glb", StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(path => path, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault() ?? string.Empty;
        }

        private static void EnsureAssetFolder(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            var normalized = path.Replace('\\', '/');
            var parts = normalized.Split('/');
            var current = parts[0];
            for (var index = 1; index < parts.Length; index++)
            {
                var next = current + "/" + parts[index];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[index]);
                }
                current = next;
            }
        }

        private static Transform EnsureGroup(Transform root, string name)
        {
            var child = root.Find(name);
            if (child != null)
            {
                ClearChildren(child);
                return child;
            }

            var group = new GameObject(name).transform;
            group.SetParent(root, false);
            return group;
        }

        private static void ClearChildren(Transform root)
        {
            for (var index = root.childCount - 1; index >= 0; index--)
            {
                UnityEngine.Object.DestroyImmediate(root.GetChild(index).gameObject);
            }
        }

        private static T GetObjectReference<T>(SerializedObject serializedObject, string propertyName) where T : UnityEngine.Object
        {
            var property = serializedObject.FindProperty(propertyName);
            return property != null ? property.objectReferenceValue as T : null;
        }

        private static Transform FindChildByName(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            var transforms = root.GetComponentsInChildren<Transform>(true);
            return transforms.FirstOrDefault(transform => string.Equals(transform.name, childName, StringComparison.Ordinal));
        }

        private enum FitAxis
        {
            WidthAndHeight,
            HeightOnly
        }
    }
}
