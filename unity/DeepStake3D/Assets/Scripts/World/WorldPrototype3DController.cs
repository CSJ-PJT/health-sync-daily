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
        [SerializeField] private Transform zoneRoot;
        [SerializeField] private Transform playerTransform;
        [SerializeField] private Transform npcTransform;
        [SerializeField] private Transform interactableTransform;
        [SerializeField] private Transform placementMarkerTransform;
        [SerializeField] private Transform placementPreviewRoot;
        [SerializeField] private Interactable3DStub primaryInteractable;
        [SerializeField] private QuestNpc3DStub questNpc;
        [SerializeField] private SettlementPlacement3DStub settlementPlacement;
        [SerializeField] private QuarterViewCameraRig quarterViewCameraRig;
        [SerializeField] private Material fieldMaterial;
        [SerializeField] private Material archiveMaterial;
        [SerializeField] private Material placementMaterial;
        [SerializeField] private Material roadMaterial;

        private WorldPrototype3DDefinition definition = new WorldPrototype3DDefinition();
        private string lastPrompt = string.Empty;

        private void Start()
        {
            definition = WorldPrototype3DDefinition.FromJson(worldPrototypeJson);
            BuildZoneLandmarks();

            if (DeepStakeGameState.Instance != null)
            {
                DeepStakeGameState.Instance.SetZone(definition.zoneId, definition.zoneLabel);
            }

            var save = DeepStakeGameState.Instance != null
                ? DeepStakeGameState.Instance.CurrentSave
                : null;

            if (playerTransform != null)
            {
                var spawnX = definition.playerSpawn.x;
                var spawnZ = definition.playerSpawn.z;
                if (save != null && save.Player.MapId == definition.zoneId)
                {
                    spawnX = save.Player.X;
                    spawnZ = save.Player.Y;
                }

                playerTransform.position = new Vector3(spawnX, 0.5f, spawnZ);
            }

            if (npcTransform != null && definition.npcStubs.Length > 0)
            {
                npcTransform.position = new Vector3(definition.npcStubs[0].x, 0.5f, definition.npcStubs[0].z);
                if (questNpc != null)
                {
                    questNpc.Configure(
                        definition.npcStubs[0].id,
                        definition.npcStubs[0].displayName,
                        definition.npcStubs[0].questId,
                        string.IsNullOrWhiteSpace(definition.npcStubs[0].dialogue)
                            ? definition.npcStubs[0].displayName + " watches the field for the first sign of recovery."
                            : definition.npcStubs[0].dialogue);
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
                definition.placementZone.x,
                0.5f,
                definition.placementZone.z);

            if (placementMarkerTransform != null)
            {
                placementMarkerTransform.position = placementPosition;
            }

            if (settlementPlacement != null)
            {
                settlementPlacement.Configure(
                    definition.placementZone.objectType,
                    definition.placementZone.label,
                    placementPosition);
                if (save != null && save.Settlement.Objects.Count > 0)
                {
                    settlementPlacement.ApplyPlacedState();
                }
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
                if (save != null)
                {
                    save.Player.MapId = definition.zoneId;
                    save.CurrentZoneId = definition.zoneId;
                    save.CurrentZoneLabel = definition.zoneLabel;
                    save.LastStatus = "Entered " + definition.zoneLabel + ".";
                    DeepStakeGameState.Instance.UpdateStatus(
                        definition.zoneLabel + ": " + definition.zoneSummary);
                }

                DeepStakeGameState.Instance.UpdatePrompt(
                    "Move through the field. Approach the sign, the archivist, or the beacon frame.");
            }
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            RefreshPrompt();

            if (keyboard.eKey.wasPressedThisFrame && primaryInteractable != null && primaryInteractable.CanInteract(playerTransform))
            {
                primaryInteractable.Trigger();
            }

            if (keyboard.qKey.wasPressedThisFrame && questNpc != null && questNpc.CanTalk(playerTransform))
            {
                questNpc.Talk();
            }

            if (keyboard.bKey.wasPressedThisFrame && settlementPlacement != null && settlementPlacement.CanPlace(playerTransform))
            {
                settlementPlacement.Place();
            }

            if (keyboard.f5Key.wasPressedThisFrame && DeepStakeGameState.Instance != null)
            {
                LocalSaveService.Save(DeepStakeGameState.Instance.CurrentSave);
                DeepStakeGameState.Instance.UpdateStatus("Saved local slot to " + LocalSaveService.GetSavePath());
            }

            if (keyboard.f9Key.wasPressedThisFrame && DeepStakeGameState.Instance != null)
            {
                var loaded = LocalSaveService.LoadOrCreate();
                DeepStakeGameState.Instance.ReplaceSave(loaded, "Reloaded local slot from disk.");
                if (playerTransform != null && loaded.Player.MapId == definition.zoneId)
                {
                    playerTransform.position = new Vector3(loaded.Player.X, 0.5f, loaded.Player.Y);
                }

                if (settlementPlacement != null && loaded.Settlement.Objects.Count > 0)
                {
                    settlementPlacement.ApplyPlacedState();
                }
            }
        }

        private void RefreshPrompt()
        {
            if (DeepStakeGameState.Instance == null)
            {
                return;
            }

            var prompt = "WASD Move  E Inspect  Q Talk  B Place  F5 Save  F9 Reload";
            if (primaryInteractable != null && primaryInteractable.CanInteract(playerTransform))
            {
                prompt = primaryInteractable.GetPrompt();
            }
            else if (questNpc != null && questNpc.CanTalk(playerTransform))
            {
                prompt = questNpc.GetPrompt();
            }
            else if (settlementPlacement != null && settlementPlacement.CanPlace(playerTransform))
            {
                prompt = settlementPlacement.GetPrompt();
            }

            if (prompt != lastPrompt)
            {
                lastPrompt = prompt;
                DeepStakeGameState.Instance.UpdatePrompt(prompt);
            }
        }

        private void BuildZoneLandmarks()
        {
            if (zoneRoot == null || definition.landmarks == null)
            {
                return;
            }

            foreach (Transform child in zoneRoot)
            {
                Destroy(child.gameObject);
            }

            for (var index = 0; index < definition.landmarks.Length; index++)
            {
                var landmark = definition.landmarks[index];
                var primitiveType = PrimitiveType.Cube;
                var material = fieldMaterial;
                var scaleY = landmark.height;

                if (landmark.kind == "road")
                {
                    primitiveType = PrimitiveType.Cube;
                    material = roadMaterial;
                    scaleY = 0.1f;
                }
                else if (landmark.kind == "archive")
                {
                    material = archiveMaterial;
                }
                else if (landmark.kind == "placement-zone")
                {
                    material = placementMaterial;
                    scaleY = 0.1f;
                }

                var item = GameObject.CreatePrimitive(primitiveType);
                item.name = landmark.label;
                item.transform.SetParent(zoneRoot, false);
                item.transform.position = new Vector3(landmark.x, scaleY * 0.5f, landmark.z);
                item.transform.localScale = new Vector3(landmark.width, scaleY, landmark.depth);

                var renderer = item.GetComponent<Renderer>();
                if (renderer != null && material != null)
                {
                    renderer.sharedMaterial = material;
                }
            }

            if (placementPreviewRoot != null)
            {
                placementPreviewRoot.position = new Vector3(definition.placementZone.x, 0.5f, definition.placementZone.z);
            }
        }
    }
}
