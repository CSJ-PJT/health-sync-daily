using System;
using DeepStake.Contracts;
using DeepStake.Core;
using DeepStake.Save;
using UnityEngine;

namespace DeepStake.Settlement
{
    public sealed class SettlementPlacementStub : MonoBehaviour
    {
        [SerializeField] private string objectType = "beacon";
        [SerializeField] private Vector2Int placementCell = new(3, -1);

        public void Configure(string nextObjectType, Vector2Int nextPlacementCell)
        {
            objectType = nextObjectType;
            placementCell = nextPlacementCell;
        }

        public void Place()
        {
            if (DeepStakeGameState.Instance == null)
            {
                return;
            }

            var save = DeepStakeGameState.Instance.CurrentSave;
            save.Settlement.Objects.Add(new DeepStakeSettlementObjectState
            {
                Id = Guid.NewGuid().ToString("N"),
                Type = objectType,
                X = placementCell.x,
                Y = placementCell.y,
            });

            if (!save.Settlement.UnlockedObjectTypes.Contains(objectType))
            {
                save.Settlement.UnlockedObjectTypes.Add(objectType);
            }

            save.LastStatus = $"Placed settlement object: {objectType}";
            DeepStakeGameState.Instance.UpdateStatus(save.LastStatus);
            LocalSaveService.Save(save);
        }
    }
}
