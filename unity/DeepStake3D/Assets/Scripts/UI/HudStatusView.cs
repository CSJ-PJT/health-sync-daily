using DeepStake.Core;
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

            var save = DeepStakeGameState.Instance.CurrentSave;
            statusText.text =
                $"Deep Stake 3D Prototype\n" +
                $"{DeepStakeGameState.Instance.ZoneLabel}\n" +
                $"Day {save.Day}  Time {save.Minutes}\n" +
                $"Energy {save.Player.Energy}/{save.Player.MaxEnergy}\n" +
                $"Quest {GetQuestSummary(save)}\n" +
                $"Settlement Objects {save.Settlement.Objects.Count}\n" +
                $"Pressure {save.WorldPressure.DominantFactionId} · Debt {save.WorldPressure.LocalDebtPressure} · Supply {save.WorldPressure.SupplyChainPressure}\n" +
                $"Prompt {DeepStakeGameState.Instance.InteractionPrompt}\n" +
                $"Status {DeepStakeGameState.Instance.StatusMessage}\n" +
                $"Save {LocalSaveService.GetSavePath()}";
        }

        private static string GetQuestSummary(Contracts.DeepStakeSaveData save)
        {
            for (var index = 0; index < save.Quests.Count; index++)
            {
                var quest = save.Quests[index];
                if (quest.Status != "completed")
                {
                    return quest.QuestId + " · " + quest.Status;
                }
            }

            return "first loop complete";
        }
    }
}
