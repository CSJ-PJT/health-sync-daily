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
        [SerializeField] private Vector3 placementPosition = new Vector3(4f, 0.5f, -2f);
        [SerializeField] private Transform placedRoot;
        [SerializeField] private float placementRange = 2.5f;

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

        public bool CanPlace(Transform actor)
        {
            if (actor == null)
            {
                return false;
            }

            var actorFlat = new Vector3(actor.position.x, placementPosition.y, actor.position.z);
            return Vector3.Distance(actorFlat, placementPosition) <= placementRange;
        }

        public string GetPrompt()
        {
            return "B Place  " + placementLabel;
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
            foreach (var existing in save.Settlement.Objects)
            {
                if (existing.Type == objectType &&
                    existing.X == Mathf.RoundToInt(placementPosition.x) &&
                    existing.Y == Mathf.RoundToInt(placementPosition.z))
                {
                    ApplyPlacedState();
                    save.LastStatus = placementLabel + " is already anchored here.";
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

            if (!save.Settlement.UnlockedObjectTypes.Contains(objectType))
            {
                save.Settlement.UnlockedObjectTypes.Add(objectType);
            }

            save.LastStatus = "Placed " + placementLabel + " in the recovery zone.";
            DeepStakeGameState.Instance.UpdateStatus(save.LastStatus);
            LocalSaveService.Save(save);
        }
    }
}
