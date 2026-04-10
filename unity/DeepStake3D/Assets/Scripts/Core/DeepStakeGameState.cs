using System;
using DeepStake.Contracts;
using UnityEngine;

namespace DeepStake.Core
{
    public sealed class DeepStakeGameState : MonoBehaviour
    {
        public static DeepStakeGameState Instance { get; private set; }

        [SerializeField] private DeepStakeSaveData currentSave = new();
        [SerializeField] private string statusMessage = "Booting local slice";
        [SerializeField] private string interactionPrompt = "Move with WASD. Approach a marker to interact.";
        [SerializeField] private string zoneLabel = "Recovery Field";

        public DeepStakeSaveData CurrentSave => currentSave;
        public string StatusMessage => statusMessage;
        public string InteractionPrompt => interactionPrompt;
        public string ZoneLabel => zoneLabel;

        public event Action StateChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void ReplaceSave(DeepStakeSaveData saveData, string nextStatus)
        {
            currentSave = saveData;
            statusMessage = nextStatus;
            zoneLabel = string.IsNullOrWhiteSpace(saveData.CurrentZoneLabel) ? zoneLabel : saveData.CurrentZoneLabel;
            if (StateChanged != null)
            {
                StateChanged.Invoke();
            }
        }

        public void UpdateStatus(string nextStatus)
        {
            statusMessage = nextStatus;
            if (StateChanged != null)
            {
                StateChanged.Invoke();
            }
        }

        public void UpdatePrompt(string nextPrompt)
        {
            interactionPrompt = nextPrompt;
            if (StateChanged != null)
            {
                StateChanged.Invoke();
            }
        }

        public void SetZone(string nextZoneId, string nextZoneLabel)
        {
            currentSave.CurrentZoneId = nextZoneId;
            currentSave.CurrentZoneLabel = nextZoneLabel;
            zoneLabel = nextZoneLabel;
            if (StateChanged != null)
            {
                StateChanged.Invoke();
            }
        }
    }
}
