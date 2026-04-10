using DeepStake.Core;
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
                $"Deep Stake Local Slice\n" +
                $"Day {save.Day}  Time {save.Minutes}\n" +
                $"Energy {save.Player.Energy}/{save.Player.MaxEnergy}\n" +
                $"{DeepStakeGameState.Instance.StatusMessage}";
        }
    }
}
