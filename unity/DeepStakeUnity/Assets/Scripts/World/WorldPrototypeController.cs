using DeepStake.Core;
using DeepStake.Save;
using UnityEngine;
using UnityEngine.InputSystem;
using DeepStake.Interaction;

namespace DeepStake.World
{
    public sealed class WorldPrototypeController : MonoBehaviour
    {
        [SerializeField] private TextAsset worldPrototypeJson = null!;
        [SerializeField] private Transform playerTransform = null!;
        [SerializeField] private Transform npcTransform = null!;
        [SerializeField] private Transform interactableTransform = null!;
        [SerializeField] private Transform settlementTransform = null!;
        [SerializeField] private InteractableStub primaryInteractable = null!;
        [SerializeField] private Quests.QuestStubNpc questNpc = null!;
        [SerializeField] private Settlement.SettlementPlacementStub settlementPlacement = null!;

        private WorldPrototypeDefinition definition = new();

        private void Start()
        {
            definition = WorldPrototypeDefinition.FromJson(worldPrototypeJson);

            if (playerTransform != null)
            {
                playerTransform.position = new Vector3(definition.playerSpawn.x, definition.playerSpawn.y, 0f);
            }

            if (npcTransform != null && definition.npcStubs.Length > 0)
            {
                npcTransform.position = new Vector3(definition.npcStubs[0].x, definition.npcStubs[0].y, 0f);
                if (questNpc != null)
                {
                    questNpc.Configure(
                        definition.npcStubs[0].id,
                        definition.npcStubs[0].questId,
                        $"{definition.npcStubs[0].displayName} waits for the first proof of recovery.");
                }
            }

            if (interactableTransform != null && definition.interactables.Length > 0)
            {
                interactableTransform.position = new Vector3(definition.interactables[0].x, definition.interactables[0].y, 0f);
                if (primaryInteractable != null)
                {
                    primaryInteractable.Configure(
                        definition.interactables[0].label,
                        definition.interactables[0].message);
                }
            }

            if (settlementTransform != null)
            {
                settlementTransform.position = new Vector3(
                    definition.settlementPlacementOrigin.x,
                    definition.settlementPlacementOrigin.y,
                    0f);
            }

            if (settlementPlacement != null)
            {
                settlementPlacement.Configure(
                    "beacon",
                    new Vector2Int(
                        Mathf.RoundToInt(definition.settlementPlacementOrigin.x),
                        Mathf.RoundToInt(definition.settlementPlacementOrigin.y)));
            }

            if (DeepStakeGameState.Instance != null)
            {
                DeepStakeGameState.Instance.UpdateStatus(
                    $"Loaded {definition.sceneId} in {definition.bootMode} mode.");
            }
        }

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
