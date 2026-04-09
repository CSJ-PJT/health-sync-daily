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

        public DeepStakeSaveData CurrentSave => currentSave;
        public string StatusMessage => statusMessage;

        public event Action? StateChanged;

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
            StateChanged?.Invoke();
        }

        public void UpdateStatus(string nextStatus)
        {
            statusMessage = nextStatus;
            StateChanged?.Invoke();
        }
    }
}
