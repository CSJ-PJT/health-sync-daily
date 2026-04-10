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
        [SerializeField] private Vector3 placementPosition = new Vector3(4f, 0.5f, -2f);
        [SerializeField] private Transform placedRoot;

        public void Configure(string nextObjectType, Vector3 nextPlacementPosition)
        {
            objectType = nextObjectType;
            placementPosition = nextPlacementPosition;
        }

        public void Place()
        {
            if (DeepStakeGameState.Instance == null)
            {
                return;
            }

            if (placedRoot != null)
            {
                placedRoot.position = placementPosition;
                placedRoot.gameObject.SetActive(true);
            }

            var save = DeepStakeGameState.Instance.CurrentSave;
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

            save.LastStatus = "Placed 3D settlement object: " + objectType;
            DeepStakeGameState.Instance.UpdateStatus(save.LastStatus);
            LocalSaveService.Save(save);
        }
    }
}
