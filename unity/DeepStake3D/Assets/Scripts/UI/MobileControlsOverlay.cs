using DeepStake.Input;
using UnityEngine;
using UnityEngine.UI;

namespace DeepStake.UI
{
    public sealed class MobileControlsOverlay : MonoBehaviour
    {
        [SerializeField] private bool forceVisibleInEditor;
        [SerializeField] private Canvas targetCanvas;

        private bool built;

        private void Start()
        {
            if (!ShouldShow())
            {
                return;
            }

            Build();
        }

        private bool ShouldShow()
        {
            return Application.isMobilePlatform || forceVisibleInEditor;
        }

        public void Configure(Canvas nextCanvas, bool nextForceVisibleInEditor)
        {
            targetCanvas = nextCanvas;
            forceVisibleInEditor = nextForceVisibleInEditor;
        }

        public void Build()
        {
            if (built)
            {
                return;
            }

            built = true;
            UiRuntimeBootstrap.EnsureEventSystem();

            if (targetCanvas == null)
            {
                targetCanvas = GetComponentInParent<Canvas>();
            }

            if (targetCanvas == null)
            {
                var canvasObject = new GameObject("MobileControlsCanvas");
                targetCanvas = canvasObject.AddComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                targetCanvas.sortingOrder = 50;
                var scaler = canvasObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 1f;
                canvasObject.AddComponent<GraphicRaycaster>();
            }
            else
            {
                targetCanvas.sortingOrder = Mathf.Max(targetCanvas.sortingOrder, 50);
                if (targetCanvas.GetComponent<CanvasScaler>() == null)
                {
                    var scaler = targetCanvas.gameObject.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920f, 1080f);
                    scaler.matchWidthOrHeight = 1f;
                }

                if (targetCanvas.GetComponent<GraphicRaycaster>() == null)
                {
                    targetCanvas.gameObject.AddComponent<GraphicRaycaster>();
                }
            }

            BuildMovementPad();
            BuildActionButtons();
            BuildTopHint();
        }

        private void BuildMovementPad()
        {
            var root = CreatePanel("MovementPad", new Vector2(260f, 260f), new Vector2(140f, 130f), new Color(0f, 0f, 0f, 0f));
            root.anchorMin = new Vector2(0f, 0f);
            root.anchorMax = new Vector2(0f, 0f);
            root.pivot = new Vector2(0f, 0f);

            var baseObject = new GameObject("StickBase");
            baseObject.transform.SetParent(root, false);
            var baseRect = baseObject.AddComponent<RectTransform>();
            baseRect.sizeDelta = new Vector2(220f, 220f);
            baseRect.anchoredPosition = new Vector2(110f, 110f);
            var baseImage = baseObject.AddComponent<Image>();
            baseImage.color = new Color(0.05f, 0.08f, 0.11f, 0.28f);
            baseImage.type = Image.Type.Simple;

            var ringObject = new GameObject("StickRing");
            ringObject.transform.SetParent(baseRect, false);
            var ringRect = ringObject.AddComponent<RectTransform>();
            ringRect.sizeDelta = new Vector2(196f, 196f);
            ringRect.anchoredPosition = Vector2.zero;
            var ringImage = ringObject.AddComponent<Image>();
            ringImage.color = new Color(0.22f, 0.3f, 0.38f, 0.44f);

            var handleObject = new GameObject("StickHandle");
            handleObject.transform.SetParent(baseRect, false);
            var handleRect = handleObject.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(92f, 92f);
            handleRect.anchoredPosition = Vector2.zero;
            var handleImage = handleObject.AddComponent<Image>();
            handleImage.color = new Color(0.72f, 0.79f, 0.84f, 0.82f);

            var innerObject = new GameObject("StickHandleCore");
            innerObject.transform.SetParent(handleRect, false);
            var innerRect = innerObject.AddComponent<RectTransform>();
            innerRect.sizeDelta = new Vector2(54f, 54f);
            innerRect.anchoredPosition = Vector2.zero;
            var innerImage = innerObject.AddComponent<Image>();
            innerImage.color = new Color(0.22f, 0.3f, 0.38f, 0.86f);

            var stick = baseObject.AddComponent<MobileVirtualStick>();
            stick.StickRoot = baseRect;
            stick.Handle = handleRect;
            stick.Radius = 64f;
        }

        private void BuildActionButtons()
        {
            var root = CreatePanel("ActionPad", new Vector2(430f, 180f), new Vector2(-24f, 30f), new Color(0f, 0f, 0f, 0.045f));
            root.anchorMin = new Vector2(1f, 0f);
            root.anchorMax = new Vector2(1f, 0f);
            root.pivot = new Vector2(1f, 0f);

            CreateActionButton(root, "Interact", "E", "Inspect", new Vector2(-374f, 118f), () => DeepStakeInputBridge.PressInteract());
            CreateActionButton(root, "Talk", "Q", "Talk", new Vector2(-288f, 118f), () => DeepStakeInputBridge.PressTalk());
            CreateActionButton(root, "Place", "B", "Place", new Vector2(-202f, 118f), () => DeepStakeInputBridge.PressPlace());
            CreateActionButton(root, "Attack", "F", "Attack", new Vector2(-116f, 118f), () => DeepStakeInputBridge.PressAttack());
            CreateHoldButton(root, "Run", "Run", new Vector2(-30f, 118f));
            CreateActionButton(root, "Save", "F5", "Save", new Vector2(-288f, 48f), () => DeepStakeInputBridge.PressSave());
            CreateActionButton(root, "Reload", "F9", "Load", new Vector2(-202f, 48f), () => DeepStakeInputBridge.PressReload());
            CreateActionButton(root, "Journal", "J", "Journal", new Vector2(-116f, 48f), () => DeepStakeInputBridge.PressJournal());
        }

        private void BuildTopHint()
        {
            var root = CreatePanel("MobileHint", new Vector2(460f, 54f), new Vector2(0f, -20f), new Color(0f, 0f, 0f, 0.22f));
            root.anchorMin = new Vector2(0.5f, 1f);
            root.anchorMax = new Vector2(0.5f, 1f);
            root.pivot = new Vector2(0.5f, 1f);
            CreateLabel(root.transform, "Mobile Controls Active", 20, TextAnchor.MiddleCenter);
        }

        private RectTransform CreatePanel(string name, Vector2 size, Vector2 anchoredPosition, Color color)
        {
            var panelObject = new GameObject(name);
            panelObject.transform.SetParent(targetCanvas.transform, false);
            var rect = panelObject.AddComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;
            var image = panelObject.AddComponent<Image>();
            image.color = color;
            return rect;
        }

        private void CreateActionButton(RectTransform parent, string name, string shortLabel, string title, Vector2 anchoredPosition, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(70f, 52f);
            rect.anchoredPosition = anchoredPosition;
            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.18f, 0.25f, 0.34f, 0.96f);
            var button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(action);

            CreateLabel(buttonObject.transform, shortLabel + "\n" + title, 13, TextAnchor.MiddleCenter);
        }

        private void CreateHoldButton(RectTransform parent, string name, string title, Vector2 anchoredPosition)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(70f, 52f);
            rect.anchoredPosition = anchoredPosition;
            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.22f, 0.32f, 0.24f, 0.96f);
            var holdButton = buttonObject.AddComponent<MobileHoldButton>();
            holdButton.PointerDown = () => DeepStakeInputBridge.SetMobileSprintHeld(true);
            holdButton.PointerUp = () => DeepStakeInputBridge.SetMobileSprintHeld(false);

            CreateLabel(buttonObject.transform, title + "\nHold", 13, TextAnchor.MiddleCenter);
        }

        private static void CreateLabel(Transform parent, string textValue, int fontSize, TextAnchor alignment)
        {
            var labelObject = new GameObject("Label");
            labelObject.transform.SetParent(parent, false);
            var rect = labelObject.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var text = labelObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.text = textValue;
        }
    }
}
