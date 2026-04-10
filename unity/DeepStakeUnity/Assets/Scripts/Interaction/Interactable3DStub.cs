using DeepStake.Core;
using UnityEngine;

namespace DeepStake.Interaction
{
    public sealed class Interactable3DStub : MonoBehaviour
    {
        [SerializeField] private string interactLabel = "Sign";
        [SerializeField] [TextArea] private string interactMessage =
            "The field waits for the first cycle of restoration.";

        public void Configure(string nextLabel, string nextMessage)
        {
            interactLabel = nextLabel;
            interactMessage = nextMessage;
        }

        public void Trigger()
        {
            if (DeepStakeGameState.Instance != null)
            {
                DeepStakeGameState.Instance.UpdateStatus(interactLabel + ": " + interactMessage);
            }
        }
    }
}
