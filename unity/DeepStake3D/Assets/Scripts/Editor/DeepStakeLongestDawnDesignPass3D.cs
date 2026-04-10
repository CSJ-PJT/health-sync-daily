#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace DeepStake.EditorTools
{
    public static class DeepStakeLongestDawnDesignPass3D
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string WorldScenePath = "Assets/Scenes/WorldPrototype3D.unity";

        [MenuItem("Tools/Deep Stake 3D/Apply Longest Dawn Design Pass")]
        public static void ApplyLongestDawnDesignPass()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    "Deep Stake 3D",
                    "Longest Dawn design pass is disabled during Play Mode. Exit Play Mode and run it again.",
                    "OK");
                return;
            }

            var report = new List<string>();
            ApplyMainMenuLorePass(report);
            ApplyWorldLorePass(report);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Deep Stake 3D",
                "Applied Longest Dawn design pass.\n\n" + string.Join("\n", report),
                "OK");
        }

        [MenuItem("Tools/Deep Stake 3D/Apply Longest Dawn Design Pass", true)]
        public static bool ValidateApplyLongestDawnDesignPass()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        private static void ApplyMainMenuLorePass(List<string> report)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainMenuScenePath);
            if (sceneAsset == null)
            {
                report.Add("- MainMenu scene not found.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                report.Add("- MainMenu canvas not found.");
                return;
            }

            var storyTag = EnsureText(canvas.transform, "StoryTag", new Vector2(0f, 118f), new Vector2(760f, 40f), 18);
            storyTag.text = "Recovery / Record / Settlement / Quiet Pressure";
            storyTag.alignment = TextAnchor.MiddleCenter;
            storyTag.color = FromHex("#9FA68F");

            var routeTag = EnsureText(canvas.transform, "RouteTag", new Vector2(0f, -190f), new Vector2(760f, 48f), 17);
            routeTag.text = "Longest Dawn opens with a field notice, an archivist, and the first beacon frame.";
            routeTag.alignment = TextAnchor.MiddleCenter;
            routeTag.color = FromHex("#B8B6AC");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            report.Add("- Added Longest Dawn story tags to MainMenu.");
        }

        private static void ApplyWorldLorePass(List<string> report)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(WorldScenePath);
            if (sceneAsset == null)
            {
                report.Add("- WorldPrototype3D scene not found.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(WorldScenePath, OpenSceneMode.Single);

            CreateOrUpdateCutePlayerProxy();
            CreateOrUpdateWorldStoryProps();
            CreateOrUpdatePressureMarkers();
            CreateOrUpdateHudLoreTags();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            report.Add("- Added cute player proxy accents, world props, pressure markers, and lore HUD tags.");
        }

        private static void CreateOrUpdateCutePlayerProxy()
        {
            var player = GameObject.Find("Player3D");
            if (player == null)
            {
                return;
            }

            player.transform.localScale = new Vector3(0.82f, 0.92f, 0.82f);

            var renderer = player.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetOrCreateMaterial("Assets/Materials/DeepStake3D/Player_CuteProxy.mat", FromHex("#8DA18A"), 0.14f);
            }

            var head = EnsurePrimitiveChild(player.transform, "CuteProxyHead", PrimitiveType.Sphere);
            head.transform.localPosition = new Vector3(0f, 0.95f, 0.12f);
            head.transform.localScale = new Vector3(0.72f, 0.66f, 0.72f);
            AssignRendererMaterial(head, GetOrCreateMaterial("Assets/Materials/DeepStake3D/Player_CuteHead.mat", FromHex("#E2D6C4"), 0.08f));

            var bag = EnsurePrimitiveChild(player.transform, "CuteProxyBag", PrimitiveType.Cube);
            bag.transform.localPosition = new Vector3(-0.18f, 0.44f, -0.26f);
            bag.transform.localScale = new Vector3(0.24f, 0.26f, 0.16f);
            AssignRendererMaterial(bag, GetOrCreateMaterial("Assets/Materials/DeepStake3D/Player_Bag.mat", FromHex("#8A6C58"), 0.04f));
        }

        private static void CreateOrUpdateWorldStoryProps()
        {
            var root = EnsureRoot("LongestDawnStoryProps");

            var ledgerCrate = CreateBox(root, "LedgerCrate", new Vector3(1.6f, 0.35f, 1.7f), new Vector3(0.9f, 0.7f, 0.7f), "#7A6A57");
            var cratePaper = EnsurePrimitiveChild(ledgerCrate.transform, "LedgerPaper", PrimitiveType.Cube);
            cratePaper.transform.localPosition = new Vector3(0f, 0.56f, 0f);
            cratePaper.transform.localScale = new Vector3(0.55f, 0.06f, 0.4f);
            AssignRendererMaterial(cratePaper, GetOrCreateMaterial("Assets/Materials/DeepStake3D/LedgerPaper.mat", FromHex("#CEC4A8"), 0.02f));

            CreateBox(root, "SupplyPallet", new Vector3(2.4f, 0.15f, 1.2f), new Vector3(1.2f, 0.3f, 0.9f), "#665A4D");
            CreateBox(root, "RoadBarrier", new Vector3(-0.6f, 0.45f, -2.7f), new Vector3(1.8f, 0.9f, 0.22f), "#5E594E");
            CreateBox(root, "FieldFenceA", new Vector3(-3.6f, 0.45f, 0.9f), new Vector3(0.18f, 0.9f, 3.2f), "#6D655A");
            CreateBox(root, "FieldFenceB", new Vector3(1.5f, 0.45f, 4.8f), new Vector3(6.0f, 0.9f, 0.18f), "#6D655A");

            var observerTower = CreateBox(root, "ObserverFrame", new Vector3(5.8f, 1.2f, 3.6f), new Vector3(0.8f, 2.4f, 0.8f), "#6F7368");
            var roof = EnsurePrimitiveChild(observerTower.transform, "RoofCap", PrimitiveType.Cube);
            roof.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            roof.transform.localScale = new Vector3(1.25f, 0.12f, 1.25f);
            AssignRendererMaterial(roof, GetOrCreateMaterial("Assets/Materials/DeepStake3D/RoofCap.mat", FromHex("#514A46"), 0.05f));
        }

        private static void CreateOrUpdatePressureMarkers()
        {
            var root = EnsureRoot("LongestDawnPressureMarkers");

            var noticeA = CreateFlatPanel(root, "SeizureNotice", new Vector3(2.7f, 1.0f, 2.7f), new Vector3(0.9f, 1.1f, 0.04f), "#C1B79A");
            var noticeAText = EnsureWorldText(noticeA.transform, "NoticeText", "NOTICE // Route audit pending", 0.16f, new Vector3(0f, 0f, 0.03f));
            noticeAText.color = FromHex("#433A34");

            var noticeB = CreateFlatPanel(root, "MissingLedgerBoard", new Vector3(4.9f, 1.1f, 1.0f), new Vector3(1.0f, 1.2f, 0.04f), "#D0C6A8");
            var noticeBText = EnsureWorldText(noticeB.transform, "LedgerText", "Archive copy incomplete", 0.16f, new Vector3(0f, 0f, 0.03f));
            noticeBText.color = FromHex("#433A34");
        }

        private static void CreateOrUpdateHudLoreTags()
        {
            var canvas = GameObject.Find("HudCanvas");
            if (canvas == null)
            {
                return;
            }

            var zoneTag = EnsureText(canvas.transform, "ZoneMoodTag", new Vector2(18f, -210f), new Vector2(520f, 32f), 16);
            zoneTag.rectTransform.anchorMin = new Vector2(0f, 1f);
            zoneTag.rectTransform.anchorMax = new Vector2(0f, 1f);
            zoneTag.rectTransform.pivot = new Vector2(0f, 1f);
            zoneTag.alignment = TextAnchor.UpperLeft;
            zoneTag.text = "Longest Dawn // first recovery edge";
            zoneTag.color = FromHex("#A9B48B");

            var pressureTag = EnsureText(canvas.transform, "PressureHintTag", new Vector2(18f, -236f), new Vector2(620f, 32f), 15);
            pressureTag.rectTransform.anchorMin = new Vector2(0f, 1f);
            pressureTag.rectTransform.anchorMax = new Vector2(0f, 1f);
            pressureTag.rectTransform.pivot = new Vector2(0f, 1f);
            pressureTag.alignment = TextAnchor.UpperLeft;
            pressureTag.text = "Pressure trace // repeated notices, delayed supplies, thinned records";
            pressureTag.color = FromHex("#C5B28A");
        }

        private static GameObject EnsureRoot(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null)
            {
                return existing;
            }

            return new GameObject(name);
        }

        private static GameObject CreateBox(GameObject root, string name, Vector3 position, Vector3 scale, string colorHex)
        {
            var obj = FindOrCreatePrimitive(root.transform, name, PrimitiveType.Cube);
            obj.transform.position = position;
            obj.transform.localScale = scale;
            AssignRendererMaterial(obj, GetOrCreateMaterial("Assets/Materials/DeepStake3D/" + name + ".mat", FromHex(colorHex), 0.06f));
            return obj;
        }

        private static GameObject CreateFlatPanel(GameObject root, string name, Vector3 position, Vector3 scale, string colorHex)
        {
            var obj = FindOrCreatePrimitive(root.transform, name, PrimitiveType.Cube);
            obj.transform.position = position;
            obj.transform.localScale = scale;
            AssignRendererMaterial(obj, GetOrCreateMaterial("Assets/Materials/DeepStake3D/" + name + ".mat", FromHex(colorHex), 0.02f));
            return obj;
        }

        private static TextMesh EnsureWorldText(Transform parent, string name, string text, float size, Vector3 localPosition)
        {
            var child = parent.Find(name);
            GameObject obj;
            if (child == null)
            {
                obj = new GameObject(name);
                obj.transform.SetParent(parent, false);
                obj.AddComponent<MeshRenderer>();
                obj.AddComponent<TextMesh>();
            }
            else
            {
                obj = child.gameObject;
            }

            obj.transform.localPosition = localPosition;
            var mesh = obj.GetComponent<TextMesh>();
            mesh.text = text;
            mesh.anchor = TextAnchor.MiddleCenter;
            mesh.alignment = TextAlignment.Center;
            mesh.characterSize = size;
            mesh.fontSize = 48;
            return mesh;
        }

        private static GameObject FindOrCreatePrimitive(Transform parent, string name, PrimitiveType type)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child.gameObject;
            }

            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static GameObject EnsurePrimitiveChild(Transform parent, string name, PrimitiveType type)
        {
            var child = parent.Find(name);
            if (child != null)
            {
                return child.gameObject;
            }

            var obj = GameObject.CreatePrimitive(type);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static void AssignRendererMaterial(GameObject target, Material material)
        {
            var renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.sharedMaterial = material;
            EditorUtility.SetDirty(renderer);
        }

        private static Material GetOrCreateMaterial(string assetPath, Color color, float smoothness)
        {
            EnsureFolder("Assets/Materials");
            EnsureFolder("Assets/Materials/DeepStake3D");

            var material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                material = new Material(ResolveDefaultShader());
                AssetDatabase.CreateAsset(material, assetPath);
            }

            material.color = color;
            if (material.HasProperty("_Smoothness"))
            {
                material.SetFloat("_Smoothness", smoothness);
            }

            if (material.HasProperty("_Metallic"))
            {
                material.SetFloat("_Metallic", 0f);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static Shader ResolveDefaultShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader != null)
            {
                return shader;
            }

            shader = Shader.Find("Standard");
            if (shader != null)
            {
                return shader;
            }

            return Shader.Find("Sprites/Default");
        }

        private static Text EnsureText(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, int fontSize)
        {
            var child = FindChildRecursive(parent, name);
            GameObject obj;
            if (child == null)
            {
                obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
                obj.transform.SetParent(parent, false);
            }
            else
            {
                obj = child.gameObject;
            }

            var rect = obj.GetComponent<RectTransform>();
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var text = obj.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            return text;
        }

        private static Transform FindChildRecursive(Transform parent, string objectName)
        {
            if (parent.name == objectName)
            {
                return parent;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);
                var match = FindChildRecursive(child, objectName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }

        private static Color FromHex(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out var color);
            return color;
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
#endif
