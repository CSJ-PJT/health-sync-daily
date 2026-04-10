using DeepStake.Core;
using UnityEngine;

namespace DeepStake.Interaction
{
    public sealed class InteractableStub : MonoBehaviour
    {
        [SerializeField] private string interactLabel = "Sign";
        [SerializeField] [TextArea] private string interactMessage =
            "The Longest Dawn is still upon this place.";

        public void Configure(string nextLabel, string nextMessage)
        {
            interactLabel = nextLabel;
            interactMessage = nextMessage;
        }

        public void Trigger()
        {
            if (DeepStakeGameState.Instance != null)
            {
                DeepStakeGameState.Instance.UpdateStatus($"{interactLabel}: {interactMessage}");
            }
        }
    }
}
