using DeepStake.Core;
using UnityEngine;

namespace DeepStake.Interaction
{
    public sealed class Interactable3DStub : MonoBehaviour
    {
        [SerializeField] private string interactLabel = "Sign";
        [SerializeField] [TextArea] private string interactMessage =
            "The field waits for the first cycle of restoration.";
        [SerializeField] private float interactionRange = 2.25f;

        public void Configure(string nextLabel, string nextMessage)
        {
            interactLabel = nextLabel;
            interactMessage = nextMessage;
        }

        public bool CanInteract(Transform actor)
        {
            if (actor == null)
            {
                return false;
            }

            var actorFlat = new Vector3(actor.position.x, transform.position.y, actor.position.z);
            return Vector3.Distance(actorFlat, transform.position) <= interactionRange;
        }

        public string GetPrompt()
        {
            return "E Inspect  " + interactLabel;
        }

        public void Trigger()
        {
            if (DeepStakeGameState.Instance != null)
            {
                var save = DeepStakeGameState.Instance.CurrentSave;
                save.Alignment.Attunement += 1;
                save.LastStatus = interactLabel + ": " + interactMessage;
                DeepStakeGameState.Instance.UpdateStatus(interactLabel + ": " + interactMessage);
            }
        }
    }
}
