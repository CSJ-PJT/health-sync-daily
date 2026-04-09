using DeepStake.Core;
using DeepStake.Save;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepStake.World
{
    public sealed class WorldPrototypeController : MonoBehaviour
    {
        [SerializeField] private Interactable.InteractableStub primaryInteractable = null!;
        [SerializeField] private Quests.QuestStubNpc questNpc = null!;
        [SerializeField] private Settlement.SettlementPlacementStub settlementPlacement = null!;

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.eKey.wasPressedThisFrame && primaryInteractable != null)
            {
                primaryInteractable.Trigger();
            }

            if (keyboard.qKey.wasPressedThisFrame && questNpc != null)
            {
                questNpc.Talk();
            }

            if (keyboard.bKey.wasPressedThisFrame && settlementPlacement != null)
            {
                settlementPlacement.Place();
            }

            if (keyboard.f5Key.wasPressedThisFrame && DeepStakeGameState.Instance != null)
            {
                LocalSaveService.Save(DeepStakeGameState.Instance.CurrentSave);
                DeepStakeGameState.Instance.UpdateStatus("Saved local prototype slot.");
            }
        }
    }
}
