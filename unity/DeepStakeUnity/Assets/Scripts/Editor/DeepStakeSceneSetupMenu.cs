#if UNITY_EDITOR
using DeepStake.Boot;
using DeepStake.Interaction;
using DeepStake.Player;
using DeepStake.Quests;
using DeepStake.Settlement;
using DeepStake.UI;
using DeepStake.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeepStake.EditorTools
{
    public static class DeepStakeSceneSetupMenu
    {
        private const string BootScenePath = "Assets/Scenes/Boot.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string WorldScenePath = "Assets/Scenes/WorldPrototype.unity";
        private const string WorldJsonPath = "Assets/Data/world-prototype.json";

        [MenuItem("Tools/Deep Stake/Create Boot Scene")]
        public static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrapObject = new GameObject("DeepStakeBootstrap");
            bootstrapObject.AddComponent<DeepStakeBootstrap>();

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, BootScenePath);
            AddScenesToBuild();
            Selection.activeObject = bootstrapObject;
        }

        [MenuItem("Tools/Deep Stake/Create Main Menu Scene")]
        public static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var canvas = CreateCanvas("MainMenuCanvas");
            var controllerObject = new GameObject("MainMenuController");
            controllerObject.AddComponent<MainMenuController>();

            var headline = CreateText("Headline", canvas.transform, new Vector2(0f, 170f), new Vector2(520f, 80f), 42, "Deep Stake");
            var status = CreateText("Status", canvas.transform, new Vector2(0f, 60f), new Vector2(680f, 120f), 22, "Local mode ready.");
            var startButton = CreateButton("StartLocalPlay", canvas.transform, new Vector2(0f, -40f), "Start Local Play");
            var continueButton = CreateButton("ContinueLatest", canvas.transform, new Vector2(0f, -120f), "Continue Latest Save");

            var controller = controllerObject.GetComponent<MainMenuController>();
            AssignObjectField(controller, "headlineText", headline.GetComponent<Text>());
            AssignObjectField(controller, "statusText", status.GetComponent<Text>());

            UnityEventTools.RemovePersistentListeners(startButton.onClick);
            UnityEventTools.RemovePersistentListeners(continueButton.onClick);
            UnityEventTools.AddPersistentListener(startButton.onClick, controller.StartLocalPlay);
            UnityEventTools.AddPersistentListener(continueButton.onClick, controller.ContinueLatest);

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
            AddScenesToBuild();
            Selection.activeObject = controllerObject;
        }

        [MenuItem("Tools/Deep Stake/Create World Prototype Scene")]
        public static void CreateWorldPrototypeScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraObject = new GameObject("Main Camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.09f, 0.14f, 1f);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            var controllerObject = new GameObject("WorldPrototypeController");
            var worldController = controllerObject.AddComponent<WorldPrototypeController>();

            var player = CreateSpriteMarker("Player", Color.cyan, new Vector3(0f, 0f, 0f));
            player.AddComponent<Rigidbody2D>().gravityScale = 0f;
            player.AddComponent<CircleCollider2D>();
            player.AddComponent<PlayerMover2D>();

            var sign = CreateSpriteMarker("FarmSign", new Color(0.8f, 0.7f, 0.3f, 1f), new Vector3(-2f, 1f, 0f));
            var interactable = sign.AddComponent<InteractableStub>();

            var npc = CreateSpriteMarker("Archivist", new Color(0.7f, 0.4f, 0.9f, 1f), new Vector3(2f, 1f, 0f));
            var questNpc = npc.AddComponent<QuestStubNpc>();

            var settlement = new GameObject("SettlementPlacementStub");
            var settlementPlacement = settlement.AddComponent<SettlementPlacementStub>();
            settlement.transform.position = new Vector3(3f, -1f, 0f);

            var canvas = CreateCanvas("HudCanvas");
            var hudText = CreateText("HudStatus", canvas.transform, new Vector2(0f, 200f), new Vector2(820f, 120f), 20, "Deep Stake HUD");
            var hudObject = new GameObject("HudStatusView");
            hudObject.transform.SetParent(canvas.transform, false);
            var hud = hudObject.AddComponent<HudStatusView>();
            AssignObjectField(hud, "statusText", hudText.GetComponent<Text>());

            AssignObjectField(worldController, "worldPrototypeJson", AssetDatabase.LoadAssetAtPath<TextAsset>(WorldJsonPath));
            AssignObjectField(worldController, "playerTransform", player.transform);
            AssignObjectField(worldController, "npcTransform", npc.transform);
            AssignObjectField(worldController, "interactableTransform", sign.transform);
            AssignObjectField(worldController, "settlementTransform", settlement.transform);
            AssignObjectField(worldController, "primaryInteractable", interactable);
            AssignObjectField(worldController, "questNpc", questNpc);
            AssignObjectField(worldController, "settlementPlacement", settlementPlacement);

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, WorldScenePath);
            AddScenesToBuild();
            Selection.activeObject = controllerObject;
        }

        [MenuItem("Tools/Deep Stake/Build Prototype Scenes")]
        public static void BuildPrototypeScenes()
        {
            CreateBootScene();
            CreateMainMenuScene();
            CreateWorldPrototypeScene();
            EditorUtility.DisplayDialog(
                "Deep Stake",
                "Boot, MainMenu, and WorldPrototype scenes were created. Open Build Settings and verify the scene order.",
                "OK");
        }

        private static Canvas CreateCanvas(string name)
        {
            var canvasObject = new GameObject(name);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
            return canvas;
        }

        private static GameObject CreateText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, int fontSize, string textValue)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            text.fontSize = fontSize;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = textValue;
            return textObject;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 anchoredPosition, string label)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);

            var rect = buttonObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(320f, 56f);
            rect.anchoredPosition = anchoredPosition;

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.18f, 0.25f, 0.34f, 0.95f);

            var button = buttonObject.AddComponent<Button>();

            var labelObject = CreateText("Label", buttonObject.transform, Vector2.zero, rect.sizeDelta, 20, label);
            labelObject.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

            return button;
        }

        private static GameObject CreateSpriteMarker(string name, Color color, Vector3 position)
        {
            var marker = new GameObject(name);
            marker.transform.position = position;
            var spriteRenderer = marker.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = CreateSquareSprite(color);
            return marker;
        }

        private static Sprite CreateSquareSprite(Color color)
        {
            var texture = new Texture2D(32, 32);
            var pixels = new Color[32 * 32];
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            texture.SetPixels(pixels);
            texture.Apply();
            return Sprite.Create(texture, new Rect(0f, 0f, 32f, 32f), new Vector2(0.5f, 0.5f), 32f);
        }

        private static void AddScenesToBuild()
        {
            var scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(WorldScenePath, true)
            };
            EditorBuildSettings.scenes = scenes;
        }

        private static void EnsureFolder(string path)
        {
            var parts = path.Split('/');
            var current = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                var next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }
                current = next;
            }
        }

        private static void AssignObjectField(Object target, string fieldName, Object value)
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
    }
}
#endif
