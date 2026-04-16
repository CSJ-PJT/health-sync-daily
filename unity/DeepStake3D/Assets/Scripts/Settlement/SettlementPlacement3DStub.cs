using System;
using DeepStake.Contracts;
using DeepStake.Core;
using DeepStake.Save;
using UnityEngine;

namespace DeepStake.Settlement
{
    public sealed class SettlementPlacement3DStub : MonoBehaviour
    {
        [SerializeField] private string objectType = "beacon";
        [SerializeField] private string placementLabel = "Recovery Beacon";
        [SerializeField] private string promptVerb = "Place";
        [SerializeField] private string requiredFlag = string.Empty;
        [SerializeField] private string restoredStructureId = string.Empty;
        [SerializeField] [TextArea] private string unlockMessage = string.Empty;
        [SerializeField] private Vector3 placementPosition = new Vector3(4f, 0.5f, -2f);
        [SerializeField] private Transform placedRoot;
        [SerializeField] private float placementRange = 5.5f;

        public void Configure(string nextObjectType, Vector3 nextPlacementPosition)
        {
            objectType = nextObjectType;
            placementPosition = nextPlacementPosition;
        }

        public void Configure(string nextObjectType, string nextPlacementLabel, Vector3 nextPlacementPosition)
        {
            objectType = nextObjectType;
            placementLabel = nextPlacementLabel;
            placementPosition = nextPlacementPosition;
        }

        public void Configure(
            string nextObjectType,
            string nextPlacementLabel,
            string nextPromptVerb,
            Vector3 nextPlacementPosition,
            string nextRequiredFlag,
            string nextRestoredStructureId,
            string nextUnlockMessage)
        {
            objectType = nextObjectType;
            placementLabel = nextPlacementLabel;
            promptVerb = nextPromptVerb;
            placementPosition = nextPlacementPosition;
            requiredFlag = nextRequiredFlag;
            restoredStructureId = nextRestoredStructureId;
            unlockMessage = nextUnlockMessage;
        }

        public bool CanPlace(Transform actor)
        {
            if (!IsUnlocked())
            {
                return false;
            }

            return GetDistance(actor) <= placementRange;
        }

        public bool IsNear(Transform actor)
        {
            return GetDistance(actor) <= placementRange;
        }

        public float GetDistance(Transform actor)
        {
            if (actor == null)
            {
                return float.MaxValue;
            }

            var actorFlat = new Vector3(actor.position.x, placementPosition.y, actor.position.z);
            return Vector3.Distance(actorFlat, placementPosition);
        }

        public string GetPrompt()
        {
            if (!IsUnlocked())
            {
                return "B Locked  " + placementLabel;
            }

            return "B " + promptVerb + "  " + placementLabel;
        }

        public string GetLabel()
        {
            return placementLabel;
        }

        public string GetBlockedReason()
        {
            if (IsUnlocked())
            {
                return "Move closer to " + placementLabel + ".";
            }

            return string.IsNullOrWhiteSpace(unlockMessage)
                ? placementLabel + " is locked."
                : unlockMessage;
        }

        public void ApplyPlacedState()
        {
            if (placedRoot != null)
            {
                placedRoot.position = placementPosition;
                placedRoot.gameObject.SetActive(true);
            }
        }

        public void Place()
        {
            if (DeepStakeGameState.Instance == null)
            {
                return;
            }

            var save = DeepStakeGameState.Instance.CurrentSave;
            if (!IsUnlocked())
            {
                save.LastStatus = string.IsNullOrWhiteSpace(unlockMessage)
                    ? placementLabel + " is locked."
                    : unlockMessage;
                DeepStakeGameState.Instance.UpdateStatus(save.LastStatus);
                return;
            }

            foreach (var existing in save.Settlement.Objects)
            {
                if (existing.Type == objectType &&
                    existing.X == Mathf.RoundToInt(placementPosition.x) &&
                    existing.Y == Mathf.RoundToInt(placementPosition.z))
                {
                    ApplyPlacedState();
                    save.LastStatus = placementLabel + " already placed.";
                    DeepStakeGameState.Instance.UpdateStatus(save.LastStatus);
                    return;
                }
            }

            if (placedRoot != null)
            {
                ApplyPlacedState();
            }

            save.Settlement.Objects.Add(new DeepStakeSettlementObjectState
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = objectType,
                X = Mathf.RoundToInt(placementPosition.x),
                Y = Mathf.RoundToInt(placementPosition.z),
            });
            save.StoryFlags.PlacedRecoveryBeacon |= objectType == "recovery-beacon" || objectType == "beacon";

            if (!save.Settlement.UnlockedObjectTypes.Contains(objectType))
            {
                save.Settlement.UnlockedObjectTypes.Add(objectType);
            }

            var restoredKey = string.IsNullOrWhiteSpace(restoredStructureId)
                ? objectType + "-anchor"
                : restoredStructureId;

            if (!save.Settlement.RestoredStructures.Contains(restoredKey))
            {
                save.Settlement.RestoredStructures.Add(restoredKey);
            }

            ApplyPlacementEffects(save);

            for (var index = 0; index < save.Quests.Count; index++)
            {
                if (save.Quests[index].QuestId != "first-harvest")
                {
                    continue;
                }

                save.Quests[index].Status = "completed";
                save.Quests[index].CompletedOnDay = save.Day;
                break;
            }

            save.LastStatus = placementLabel + " placed.";
            DeepStakeGameState.Instance.UpdateStatus(save.LastStatus);
            LocalSaveService.Save(save);
        }

        public bool IsUnlocked()
        {
            if (DeepStakeGameState.Instance == null || string.IsNullOrWhiteSpace(requiredFlag))
            {
                return true;
            }

            var flags = DeepStakeGameState.Instance.CurrentSave.StoryFlags;
            switch (requiredFlag)
            {
                case "ReviewedSupplyCrate":
                    return flags.ReviewedSupplyCrate;
                case "ReadFieldNoticeBoard":
                    return flags.ReadFieldNoticeBoard;
                case "MetArchivist":
                    return flags.MetArchivist;
                case "PlacedRecoveryBeacon":
                    return flags.PlacedRecoveryBeacon;
                default:
                    return true;
            }
        }

        private void ApplyPlacementEffects(DeepStakeSaveData save)
        {
            if (objectType == "recovery-beacon" || objectType == "beacon")
            {
                save.StoryFlags.PlacedRecoveryBeacon = true;
                save.WorldPressure.ResonanceSuppression = Mathf.Max(0, save.WorldPressure.ResonanceSuppression - 4);
                save.WorldPressure.SettlementInfluencePressure = Mathf.Max(0, save.WorldPressure.SettlementInfluencePressure - 5);
                save.WorldPressure.LocalDebtPressure = Mathf.Max(0, save.WorldPressure.LocalDebtPressure - 2);
                save.ActivePressureHint = "The first beacon is anchored. Pressure still surrounds Longest Dawn, but the settlement now holds one stable recovery point.";
                save.Alignment.ResonanceStability += 2;
                save.Alignment.LuminousAffinity += 1;
                return;
            }

            if (objectType == "supply-relay")
            {
                save.WorldPressure.SupplyChainPressure = Mathf.Max(0, save.WorldPressure.SupplyChainPressure - 5);
                save.WorldPressure.SettlementInfluencePressure = Mathf.Max(0, save.WorldPressure.SettlementInfluencePressure - 2);
                save.ActivePressureHint = "A supply relay now stabilizes the field lane. Longest Dawn still strains under Directorate pressure, but local routing is no longer blind.";
                save.Alignment.AwakeningClarity += 1;
                save.Alignment.Attunement += 1;
            }
        }
    }
}
