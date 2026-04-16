using DeepStake.Core;
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

        private void Start()
        {
            UiRuntimeBootstrap.EnsureEventSystem();
            QuestCatalog.Load(Resources.Load<TextAsset>("quests-longest-dawn"));
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
                "Controls  WASD move | E inspect | Q talk | B place | F5 save | F9 reload\n" +
                "Android  on-screen controls appear automatically on mobile builds\n" +
                "Status  " + DeepStakeGameState.Instance.StatusMessage;
        }

        private static string GetQuestSummary(Contracts.DeepStakeSaveData save)
        {
            return QuestCatalog.GetPrimaryMissionObjective(save);
        }
    }
}
