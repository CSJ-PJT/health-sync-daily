using DeepStake.Core;
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
            Refresh();
        }

        public void StartLocalPlay()
        {
            if (DeepStakeGameState.Instance != null)
            {
                DeepStakeGameState.Instance.UpdateStatus("Starting local quarter-view prototype world.");
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
                headlineText.text = "Deep Stake\nQuarter-View Recovery Slice";
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
                $"Mode  {save.BootMode}\n" +
                $"Zone  {save.CurrentZoneLabel}\n" +
                $"Day  {save.Day}\n" +
                $"Continue  latest local slot\n" +
                $"Controls  WASD move · E inspect · Q talk · B place · F5 save · F9 reload\n" +
                $"Status  {DeepStakeGameState.Instance.StatusMessage}";
        }
    }
}
