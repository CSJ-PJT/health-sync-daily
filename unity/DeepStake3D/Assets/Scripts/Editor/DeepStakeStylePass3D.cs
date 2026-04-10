#if UNITY_EDITOR
using System.Collections.Generic;
using DeepStake.Player;
using DeepStake.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace DeepStake.EditorTools
{
    public static class DeepStakeStylePass3D
    {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string WorldScenePath = "Assets/Scenes/WorldPrototype3D.unity";
        private const string GeneratedMaterialsFolder = "Assets/Materials/DeepStake3D";

        [MenuItem("Tools/Deep Stake 3D/Apply Survival Style Pass")]
        public static void ApplySurvivalStylePass()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorUtility.DisplayDialog(
                    "Deep Stake 3D",
                    "Style pass is disabled during Play Mode. Exit Play Mode and run it again.",
                    "OK");
                return;
            }

            EnsureFolder("Assets/Materials");
            EnsureFolder(GeneratedMaterialsFolder);

            var report = new List<string>();
            ApplyMainMenuStyle(report);
            ApplyWorldStyle(report);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Deep Stake 3D",
                "Applied survival style pass.\n\n" + string.Join("\n", report),
                "OK");
        }

        [MenuItem("Tools/Deep Stake 3D/Apply Survival Style Pass", true)]
        public static bool ValidateApplySurvivalStylePass()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        private static void ApplyMainMenuStyle(List<string> report)
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

            var background = EnsureFullscreenPanel(canvas.transform, "BackgroundPanel");
            var backgroundImage = background.GetComponent<Image>();
            backgroundImage.color = FromHex("#191D20");
            background.transform.SetAsFirstSibling();

            StyleNamedText(canvas.transform, "Headline", 42, TextAnchor.MiddleCenter, FromHex("#E7E3D7"));
            StyleNamedText(canvas.transform, "Status", 22, TextAnchor.MiddleCenter, FromHex("#B8B6AC"));
            StyleButton(canvas.transform, "StartLocalPlay");
            StyleButton(canvas.transform, "ContinueLatest");

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            report.Add("- Styled MainMenu UI with muted survival palette.");
        }

        private static void ApplyWorldStyle(List<string> report)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(WorldScenePath);
            if (sceneAsset == null)
            {
                report.Add("- WorldPrototype3D scene not found.");
                return;
            }

            var scene = EditorSceneManager.OpenScene(WorldScenePath, OpenSceneMode.Single);
            var camera = Object.FindObjectOfType<Camera>();
            if (camera != null)
            {
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = FromHex("#1E2428");
                camera.fieldOfView = 44f;
            }

            var light = Object.FindObjectOfType<Light>();
            if (light != null)
            {
                light.intensity = 0.82f;
                light.color = FromHex("#D7D0BF");
            }

            var fieldMaterial = GetOrCreateMaterial("Field_Recovery.mat", FromHex("#5B6550"), 0.35f);
            var roadMaterial = GetOrCreateMaterial("Road_Asphalt.mat", FromHex("#33373A"), 0.05f);
            var archiveMaterial = GetOrCreateMaterial("Archive_Concrete.mat", FromHex("#706D67"), 0.22f);
            var placementMaterial = GetOrCreateMaterial("Placement_Ghost.mat", FromHex("#7B6E4C"), 0.12f);
            var playerMaterial = GetOrCreateMaterial("Player_Proxy.mat", FromHex("#7A8074"), 0.18f);
            var npcMaterial = GetOrCreateMaterial("Npc_Proxy.mat", FromHex("#A69B7F"), 0.15f);
            var propMaterial = GetOrCreateMaterial("Prop_Rusted.mat", FromHex("#5A4E45"), 0.08f);

            ApplyMaterialByName("Ground", fieldMaterial);
            ApplyMaterialByName("Player3D", playerMaterial);
            ApplyMaterialByName("Archivist3D", npcMaterial);
            ApplyMaterialByName("FarmSign3D", propMaterial);
            ApplyMaterialByName("PlacementMarker3D", placementMaterial);
            ApplyMaterialByName("PlacedBeacon3D", propMaterial);

            var worldController = Object.FindObjectOfType<WorldPrototype3DController>();
            if (worldController != null)
            {
                AssignMaterialField(worldController, "fieldMaterial", fieldMaterial);
                AssignMaterialField(worldController, "archiveMaterial", archiveMaterial);
                AssignMaterialField(worldController, "placementMaterial", placementMaterial);
                AssignMaterialField(worldController, "roadMaterial", roadMaterial);
            }

            var mover = Object.FindObjectOfType<PlayerMover3D>();
            if (mover != null)
            {
                var serializedMover = new SerializedObject(mover);
                SetFloat(serializedMover, "moveSpeed", 4.2f);
                SetFloat(serializedMover, "acceleration", 9.5f);
                SetFloat(serializedMover, "turnSpeed", 8.5f);
                SetFloat(serializedMover, "gravity", 24f);
                serializedMover.ApplyModifiedPropertiesWithoutUndo();
            }

            var hudCanvas = GameObject.Find("HudCanvas");
            if (hudCanvas != null)
            {
                var hudPanel = EnsureAnchoredPanel(hudCanvas.transform, "HudPanel");
                var hudPanelImage = hudPanel.GetComponent<Image>();
                hudPanelImage.color = new Color(0.07f, 0.08f, 0.09f, 0.78f);
                hudPanel.transform.SetAsFirstSibling();

                var hudStatus = FindChildRecursive(hudCanvas.transform, "HudStatus");
                if (hudStatus != null)
                {
                    var rect = hudStatus.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchorMin = new Vector2(0f, 1f);
                        rect.anchorMax = new Vector2(0f, 1f);
                        rect.pivot = new Vector2(0f, 1f);
                        rect.sizeDelta = new Vector2(520f, 220f);
                        rect.anchoredPosition = new Vector2(18f, -18f);
                    }

                    var text = hudStatus.GetComponent<Text>();
                    if (text != null)
                    {
                        text.alignment = TextAnchor.UpperLeft;
                        text.fontSize = 18;
                        text.color = FromHex("#D8D3C7");
                    }
                }
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            report.Add("- Styled WorldPrototype3D camera, lighting, HUD, proxy materials, and movement feel.");
        }

        private static Material GetOrCreateMaterial(string fileName, Color color, float smoothness)
        {
            var path = GeneratedMaterialsFolder + "/" + fileName;
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(ResolveDefaultShader());
                AssetDatabase.CreateAsset(material, path);
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

        private static void ApplyMaterialByName(string objectName, Material material)
        {
            var target = GameObject.Find(objectName);
            if (target == null)
            {
                return;
            }

            var renderer = target.GetComponent<Renderer>();
            if (renderer == null)
            {
                return;
            }

            renderer.sharedMaterial = material;
            EditorUtility.SetDirty(renderer);
        }

        private static void StyleNamedText(Transform root, string objectName, int fontSize, TextAnchor alignment, Color color)
        {
            var target = FindChildRecursive(root, objectName);
            if (target == null)
            {
                return;
            }

            var text = target.GetComponent<Text>();
            if (text == null)
            {
                return;
            }

            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = color;
            EditorUtility.SetDirty(text);
        }

        private static void StyleButton(Transform root, string objectName)
        {
            var target = FindChildRecursive(root, objectName);
            if (target == null)
            {
                return;
            }

            var image = target.GetComponent<Image>();
            if (image != null)
            {
                image.color = FromHex("#4E5A5D");
                EditorUtility.SetDirty(image);
            }

            var button = target.GetComponent<Button>();
            if (button != null)
            {
                var colors = button.colors;
                colors.normalColor = FromHex("#4E5A5D");
                colors.highlightedColor = FromHex("#647173");
                colors.pressedColor = FromHex("#394244");
                colors.selectedColor = FromHex("#647173");
                colors.disabledColor = FromHex("#2B3133");
                button.colors = colors;
                EditorUtility.SetDirty(button);
            }

            var label = FindChildRecursive(target, "Label");
            if (label != null)
            {
                var text = label.GetComponent<Text>();
                if (text != null)
                {
                    text.color = FromHex("#F0EBDF");
                    text.fontSize = 20;
                    text.alignment = TextAnchor.MiddleCenter;
                    EditorUtility.SetDirty(text);
                }
            }
        }

        private static GameObject EnsureFullscreenPanel(Transform parent, string objectName)
        {
            var existing = FindChildRecursive(parent, objectName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            var panel = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return panel;
        }

        private static GameObject EnsureAnchoredPanel(Transform parent, string objectName)
        {
            var existing = FindChildRecursive(parent, objectName);
            if (existing != null)
            {
                return existing.gameObject;
            }

            var panel = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.sizeDelta = new Vector2(560f, 240f);
            rect.anchoredPosition = new Vector2(10f, -10f);
            return panel;
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

        private static void AssignMaterialField(Object target, string fieldName, Material value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                return;
            }

            property.objectReferenceValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void SetFloat(SerializedObject serializedObject, string fieldName, float value)
        {
            var property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                return;
            }

            property.floatValue = value;
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
