using DeepStake.CameraRig;
using DeepStake.Core;
using DeepStake.Interaction;
using DeepStake.Quests;
using DeepStake.Save;
using DeepStake.Settlement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepStake.World
{
    public sealed class WorldPrototype3DController : MonoBehaviour
    {
        [SerializeField] private TextAsset worldPrototypeJson;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform npcTransform;
        [SerializeField] private Transform interactableTransform;
        [SerializeField] private Transform placementMarkerTransform;
        [SerializeField] private Interactable3DStub primaryInteractable;
        [SerializeField] private QuestNpc3DStub questNpc;
        [SerializeField] private SettlementPlacement3DStub settlementPlacement;
        [SerializeField] private QuarterViewCameraRig quarterViewCameraRig;

        private WorldPrototype3DDefinition definition = new WorldPrototype3DDefinition();

        private void Start()
        {
            definition = WorldPrototype3DDefinition.FromJson(worldPrototypeJson);

            if (playerTransform != null)
            {
                playerTransform.position = new Vector3(definition.playerSpawn.x, 0.5f, definition.playerSpawn.z);
            }

            if (npcTransform != null && definition.npcStubs.Length > 0)
            {
                npcTransform.position = new Vector3(definition.npcStubs[0].x, 0.5f, definition.npcStubs[0].z);
                if (questNpc != null)
                {
                    questNpc.Configure(
                        definition.npcStubs[0].id,
                        definition.npcStubs[0].questId,
                        definition.npcStubs[0].displayName + " watches the field for the first sign of recovery.");
                }
            }

            if (interactableTransform != null && definition.interactables.Length > 0)
            {
                interactableTransform.position = new Vector3(definition.interactables[0].x, 0.75f, definition.interactables[0].z);
                if (primaryInteractable != null)
                {
                    primaryInteractable.Configure(
                        definition.interactables[0].label,
                        definition.interactables[0].message);
                }
            }

            var placementPosition = new Vector3(
                definition.settlementPlacementOrigin.x,
                0.5f,
                definition.settlementPlacementOrigin.z);

            if (placementMarkerTransform != null)
            {
                placementMarkerTransform.position = placementPosition;
            }

            if (settlementPlacement != null)
            {
                settlementPlacement.Configure("beacon", placementPosition);
            }

            if (quarterViewCameraRig != null && playerTransform != null)
            {
                quarterViewCameraRig.Configure(
                    playerTransform,
                    new Vector3(
                        definition.cameraOffset.x,
                        definition.cameraOffset.y,
                        definition.cameraOffset.z));
            }

            if (DeepStakeGameState.Instance != null)
            {
                DeepStakeGameState.Instance.UpdateStatus(
                    "Loaded 3D quarter-view prototype in " + definition.bootMode + " mode.");
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
                DeepStakeGameState.Instance.UpdateStatus("Saved local 3D prototype slot.");
            }
        }
    }
}
