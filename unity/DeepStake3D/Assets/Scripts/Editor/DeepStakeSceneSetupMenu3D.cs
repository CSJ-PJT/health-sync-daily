#if UNITY_EDITOR
using DeepStake.Boot;
using DeepStake.CameraRig;
using DeepStake.Interaction;
using DeepStake.Player;
using DeepStake.Quests;
using DeepStake.Settlement;
using DeepStake.UI;
using DeepStake.World;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace DeepStake.EditorTools
{
    public static class DeepStakeSceneSetupMenu3D
    {
        private const string BootScenePath = "Assets/Scenes/Boot.unity";
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";
        private const string WorldScene3DPath = "Assets/Scenes/WorldPrototype3D.unity";
        private const string WorldJson3DPath = "Assets/Data/world-prototype-3d.json";

        [MenuItem("Tools/Deep Stake 3D/Build Quarter-View Prototype Scenes")]
        public static void BuildQuarterViewPrototypeScenes()
        {
            CreateBootScene();
            CreateMainMenuScene();
            CreateWorldPrototype3DScene();

            EditorUtility.DisplayDialog(
                "Deep Stake 3D",
                "Boot, MainMenu, and WorldPrototype3D scenes were created for the quarter-view path.",
                "OK");
        }

        [MenuItem("Tools/Deep Stake 3D/Create WorldPrototype3D Scene")]
        public static void CreateWorldPrototype3DScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var directionalLight = new GameObject("Directional Light");
            var light = directionalLight.AddComponent<Light>();
            light.type = LightType.Directional;
            directionalLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            var cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.09f, 0.11f, 0.14f, 1f);
            camera.fieldOfView = 50f;
            var rig = cameraObject.AddComponent<QuarterViewCameraRig>();

            var controllerObject = new GameObject("WorldPrototype3DController");
            var worldController = controllerObject.AddComponent<WorldPrototype3DController>();

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(2.5f, 1f, 2.5f);

            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player3D";
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.AddComponent<CharacterController>();
            player.AddComponent<PlayerMover3D>();

            var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
            sign.name = "FarmSign3D";
            sign.transform.position = new Vector3(-3f, 0.75f, 2f);
            sign.transform.localScale = new Vector3(0.8f, 1.5f, 0.2f);
            var interactable = sign.AddComponent<Interactable3DStub>();

            var npc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            npc.name = "Archivist3D";
            npc.transform.position = new Vector3(3f, 1f, 2f);
            npc.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
            var questNpc = npc.AddComponent<QuestNpc3DStub>();

            var placementMarker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            placementMarker.name = "PlacementMarker3D";
            placementMarker.transform.position = new Vector3(4f, 0.2f, -2f);
            placementMarker.transform.localScale = new Vector3(0.7f, 0.15f, 0.7f);

            var placedBeacon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            placedBeacon.name = "PlacedBeacon3D";
            placedBeacon.transform.position = new Vector3(4f, 0.5f, -2f);
            placedBeacon.transform.localScale = new Vector3(0.5f, 1.2f, 0.5f);
            placedBeacon.SetActive(false);

            var settlementPlacement = placementMarker.AddComponent<SettlementPlacement3DStub>();
            AssignObjectField(settlementPlacement, "placedRoot", placedBeacon.transform);

            var canvas = CreateCanvas("HudCanvas");
            var hudText = CreateText("HudStatus", canvas.transform, new Vector2(0f, 200f), new Vector2(900f, 120f), 20, "Deep Stake 3D HUD");
            var hudObject = new GameObject("HudStatusView");
            hudObject.transform.SetParent(canvas.transform, false);
            var hud = hudObject.AddComponent<HudStatusView>();
            AssignObjectField(hud, "statusText", hudText.GetComponent<Text>());

            AssignObjectField(worldController, "worldPrototypeJson", AssetDatabase.LoadAssetAtPath<TextAsset>(WorldJson3DPath));
            AssignObjectField(worldController, "playerTransform", player.transform);
            AssignObjectField(worldController, "npcTransform", npc.transform);
            AssignObjectField(worldController, "interactableTransform", sign.transform);
            AssignObjectField(worldController, "placementMarkerTransform", placementMarker.transform);
            AssignObjectField(worldController, "primaryInteractable", interactable);
            AssignObjectField(worldController, "questNpc", questNpc);
            AssignObjectField(worldController, "settlementPlacement", settlementPlacement);
            AssignObjectField(worldController, "quarterViewCameraRig", rig);

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, WorldScene3DPath);
            AddScenesToBuild();
            Selection.activeObject = controllerObject;
        }

        private static void CreateBootScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrapObject = new GameObject("DeepStakeBootstrap");
            var bootstrap = bootstrapObject.AddComponent<DeepStakeBootstrap>();
            AssignBoolField(bootstrap, "loadWorldDirectly", false);
            AssignStringField(bootstrap, "mainMenuSceneName", "MainMenu");
            AssignStringField(bootstrap, "worldSceneName", "WorldPrototype3D");

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, BootScenePath);
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var canvas = CreateCanvas("MainMenuCanvas");
            var controllerObject = new GameObject("MainMenuController");
            var controller = controllerObject.AddComponent<MainMenuController>();
            AssignStringField(controller, "worldSceneName", "WorldPrototype3D");

            var headline = CreateText("Headline", canvas.transform, new Vector2(0f, 170f), new Vector2(520f, 80f), 42, "Deep Stake");
            var status = CreateText("Status", canvas.transform, new Vector2(0f, 60f), new Vector2(700f, 120f), 22, "Quarter-view local mode ready.");
            var startButton = CreateButton("StartLocalPlay", canvas.transform, new Vector2(0f, -40f), "Start Local Play");
            var continueButton = CreateButton("ContinueLatest", canvas.transform, new Vector2(0f, -120f), "Continue Latest Save");

            AssignObjectField(controller, "headlineText", headline.GetComponent<Text>());
            AssignObjectField(controller, "statusText", status.GetComponent<Text>());

            ClearPersistentListeners(startButton);
            ClearPersistentListeners(continueButton);
            UnityEventTools.AddPersistentListener(startButton.onClick, controller.StartLocalPlay);
            UnityEventTools.AddPersistentListener(continueButton.onClick, controller.ContinueLatest);

            EnsureFolder("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, MainMenuScenePath);
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

        private static void AddScenesToBuild()
        {
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(BootScenePath, true),
                new EditorBuildSettingsScene(MainMenuScenePath, true),
                new EditorBuildSettingsScene(WorldScene3DPath, true)
            };
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

        private static void AssignStringField(Object target, string fieldName, string value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                return;
            }

            property.stringValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void AssignBoolField(Object target, string fieldName, bool value)
        {
            var serializedObject = new SerializedObject(target);
            var property = serializedObject.FindProperty(fieldName);
            if (property == null)
            {
                return;
            }

            property.boolValue = value;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ClearPersistentListeners(Button button)
        {
            for (var i = button.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            {
                UnityEventTools.RemovePersistentListener(button.onClick, i);
            }
        }
    }
}
#endif
