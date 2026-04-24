using DeepStake.Core;
using DeepStake.Characters;
using DeepStake.Quests;
using DeepStake.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DeepStake.UI
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [SerializeField] private string worldSceneName = "WorldPrototype3D";
        [SerializeField] private Text headlineText;
        [SerializeField] private Text statusText;
        private Text customizationText;

        private void Start()
        {
            UiRuntimeBootstrap.EnsureEventSystem();
            QuestCatalog.Load(Resources.Load<TextAsset>("quests-longest-dawn"));
            EnsureCustomizationPanel();
            Debug.Log("[DeepStakeDev] MainMenu ready.");
            Refresh();
        }

        public void StartLocalPlay()
        {
            if (DeepStakeGameState.Instance != null)
            {
                DeepStakeGameState.Instance.ReplaceSave(
                    LocalSaveService.CreateDefault(),
                    "Starting a fresh Longest Dawn local slice.");
            }

            SceneManager.LoadScene(worldSceneName);
        }

        public void ContinueLatest()
        {
            var saveData = LocalSaveService.LoadOrCreate();
            if (DeepStakeGameState.Instance != null)
            {
                DeepStakeGameState.Instance.ReplaceSave(saveData, "Loaded latest local save.");
            }

            SceneManager.LoadScene(worldSceneName);
        }

        private void Refresh()
        {
            if (headlineText != null)
            {
                headlineText.text = "Deep Stake\nLongest Dawn Recovery Slice";
            }

            if (statusText == null)
            {
                return;
            }

            if (DeepStakeGameState.Instance == null)
            {
                statusText.text = "Boot state not found.";
                return;
            }

            var save = DeepStakeGameState.Instance.CurrentSave;
            statusText.text =
                "Mode  " + save.BootMode + "\n" +
                "Zone  " + save.CurrentZoneLabel + "\n" +
                "Day  " + save.Day + "\n" +
                "Quest  " + GetQuestSummary(save) + "\n" +
                "Pressure  " + save.ActivePressureHint + "\n" +
                "Restored  " + save.Settlement.RestoredStructures.Count + " local anchors\n" +
                "Latest Save  " + (LocalSaveService.Exists() ? "available" : "none") + "\n" +
                "Continue  latest local slot\n" +
                "Controls  WASD move | Shift run | E inspect | Q talk | B place | F5 save | F9 reload\n" +
                "Android  on-screen controls appear automatically on mobile builds\n" +
                "Status  " + DeepStakeGameState.Instance.StatusMessage;

            RefreshCustomizationText();
        }

        private void EnsureCustomizationPanel()
        {
            if (customizationText != null)
            {
                return;
            }

            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                return;
            }

            var existing = canvas.transform.Find("CharacterCustomizationPanel");
            if (existing != null)
            {
                customizationText = existing.Find("Summary")?.GetComponent<Text>();
                return;
            }

            var panelObject = new GameObject("CharacterCustomizationPanel");
            panelObject.transform.SetParent(canvas.transform, false);
            var panelRect = panelObject.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0f);
            panelRect.anchorMax = new Vector2(0f, 0f);
            panelRect.pivot = new Vector2(0f, 0f);
            panelRect.anchoredPosition = new Vector2(28f, 28f);
            panelRect.sizeDelta = new Vector2(420f, 268f);

            var panelImage = panelObject.AddComponent<Image>();
            panelImage.color = new Color(0.055f, 0.065f, 0.07f, 0.86f);

            CreateRuntimeText("Title", panelObject.transform, new Vector2(16f, 228f), new Vector2(388f, 28f), 19, TextAnchor.MiddleLeft, "Character Setup");
            customizationText = CreateRuntimeText("Summary", panelObject.transform, new Vector2(16f, 192f), new Vector2(388f, 42f), 15, TextAnchor.UpperLeft, string.Empty);

            CreateRuntimeButton(panelObject.transform, "FaceButton", new Vector2(16f, 148f), "Face", () => { CharacterCustomizationStore.CycleFace(); RefreshCustomizationText(); });
            CreateRuntimeButton(panelObject.transform, "EyesButton", new Vector2(146f, 148f), "Eyes", () => { CharacterCustomizationStore.CycleEye(); RefreshCustomizationText(); });
            CreateRuntimeButton(panelObject.transform, "HairButton", new Vector2(276f, 148f), "Hair", () => { CharacterCustomizationStore.CycleHair(); RefreshCustomizationText(); });
            CreateRuntimeButton(panelObject.transform, "EarsButton", new Vector2(16f, 98f), "Ears", () => { CharacterCustomizationStore.CycleEar(); RefreshCustomizationText(); });
            CreateRuntimeButton(panelObject.transform, "NoseButton", new Vector2(146f, 98f), "Nose", () => { CharacterCustomizationStore.CycleNose(); RefreshCustomizationText(); });
            CreateRuntimeButton(panelObject.transform, "ClothesButton", new Vector2(276f, 98f), "Clothes", () => { CharacterCustomizationStore.CycleClothes(); RefreshCustomizationText(); });
            CreateRuntimeButton(panelObject.transform, "ShoesButton", new Vector2(16f, 48f), "Shoes", () => { CharacterCustomizationStore.CycleShoes(); RefreshCustomizationText(); });
            CreateRuntimeButton(panelObject.transform, "ResetButton", new Vector2(146f, 48f), "Reset", () => { CharacterCustomizationStore.Reset(); RefreshCustomizationText(); });
        }

        private void RefreshCustomizationText()
        {
            if (customizationText == null)
            {
                return;
            }

            var profile = CharacterCustomizationStore.Load();
            customizationText.text = CharacterCustomizationStore.Describe(profile) + "\n" + CharacterCustomizationStore.LimitationsNote();
        }

        private static Text CreateRuntimeText(string name, Transform parent, Vector2 anchoredPosition, Vector2 size, int fontSize, TextAnchor alignment, string textValue)
        {
            var textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            var rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            var text = textObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = new Color(0.88f, 0.87f, 0.82f);
            text.text = textValue;
            return text;
        }

        private static void CreateRuntimeButton(Transform parent, string name, Vector2 anchoredPosition, string label, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(name);
            buttonObject.transform.SetParent(parent, false);
            var rect = buttonObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(112f, 36f);

            var image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.18f, 0.22f, 0.23f, 0.96f);

            var button = buttonObject.AddComponent<Button>();
            button.onClick.AddListener(action);

            var labelText = CreateRuntimeText("Label", buttonObject.transform, Vector2.zero, rect.sizeDelta, 15, TextAnchor.MiddleCenter, label);
            labelText.color = new Color(0.92f, 0.90f, 0.83f);
        }

        private static string GetQuestSummary(Contracts.DeepStakeSaveData save)
        {
            return QuestCatalog.GetPrimaryMissionObjective(save);
        }
    }
}
