using DeepStake.Core;
using DeepStake.Quests;
using DeepStake.Save;
using UnityEngine;
using UnityEngine.UI;

namespace DeepStake.UI
{
    public sealed class HudStatusView : MonoBehaviour
    {
        [SerializeField] private Text statusText;

        private void OnEnable()
        {
            if (DeepStakeGameState.Instance != null)
            {
                DeepStakeGameState.Instance.StateChanged += Refresh;
                Refresh();
            }
        }

        private void OnDisable()
        {
            if (DeepStakeGameState.Instance != null)
            {
                DeepStakeGameState.Instance.StateChanged -= Refresh;
            }
        }

        private void Refresh()
        {
            if (statusText == null || DeepStakeGameState.Instance == null)
            {
                return;
            }

            if (Application.isMobilePlatform)
            {
                statusText.text = string.Empty;
                return;
            }

            var save = DeepStakeGameState.Instance.CurrentSave;
            statusText.text =
                DeepStakeGameState.Instance.ZoneLabel +
                " | Day " + save.Day +
                " | Energy " + save.Player.Energy + "\n" +
                "Nearby " + DeepStakeGameState.Instance.NearbyTargetLabel + "\n" +
                "Status " + DeepStakeGameState.Instance.StatusMessage;
        }

        private static string GetQuestSummary(Contracts.DeepStakeSaveData save)
        {
            return QuestCatalog.GetPrimaryMissionTitle(save) + " | " +
                QuestCatalog.GetPrimaryMissionObjective(save);
        }
    }
}
