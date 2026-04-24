using DeepStake.Core;
using DeepStake.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeepStake.Boot
{
    public sealed class DeepStakeBootstrap : MonoBehaviour
    {
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        [SerializeField] private string worldSceneName = "WorldPrototype3D";
        [SerializeField] private bool forceLocalMode = true;
        [SerializeField] private bool loadWorldDirectly = false;

        private void Awake()
        {
            if (FindFirstObjectByType<DeepStakeGameState>() == null)
            {
                var stateObject = new GameObject("DeepStakeGameState");
                stateObject.AddComponent<DeepStakeGameState>();
            }
        }

        private void Start()
        {
            if (DeepStakeDevLaunchOptions.AutorunLocalPlay)
            {
                var devSave = LocalSaveService.CreateDefault();
                devSave.BootMode = "local-dev-autorun";
                devSave.LastStatus = "Dev autorun active. Entering playable field directly.";
                DeepStakeGameState.Instance.ReplaceSave(devSave, devSave.LastStatus);
                Debug.Log("[DeepStakeDev] Boot autorun -> " + worldSceneName + " tag=" + DeepStakeDevLaunchOptions.VerificationTag);
                SceneManager.LoadScene(worldSceneName);
                return;
            }

            var saveData = LocalSaveService.LoadOrCreate();
            saveData.BootMode = forceLocalMode ? "local" : saveData.BootMode;
            saveData.LastStatus = forceLocalMode
                ? "Cloud link optional. Running in local quarter-view recovery mode."
                : "Boot complete.";

            DeepStakeGameState.Instance.ReplaceSave(saveData, saveData.LastStatus);
            Debug.Log("[DeepStakeDev] Boot normal flow -> " + (loadWorldDirectly ? worldSceneName : mainMenuSceneName));
            SceneManager.LoadScene(loadWorldDirectly ? worldSceneName : mainMenuSceneName);
        }
    }
}
