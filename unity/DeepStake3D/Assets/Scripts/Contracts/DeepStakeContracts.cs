using System;
using System.Collections.Generic;

namespace DeepStake.Contracts
{
    public enum DeepStakeBootMode
    {
        Cloud,
        Local,
        Degraded
    }

    [Serializable]
    public sealed class DeepStakeInventoryEntry
    {
        public string ItemId = string.Empty;
        public int Amount;
    }

    [Serializable]
    public sealed class DeepStakePlayerState
    {
        public string MapId = "farm";
        public float X;
        public float Y;
        public string Facing = "down";
        public int Energy = 100;
        public int MaxEnergy = 100;
        public List<DeepStakeInventoryEntry> Inventory = new();
        public List<string> Hotbar = new();
        public int SelectedHotbarIndex;
    }

    [Serializable]
    public sealed class DeepStakeQuestState
    {
        public string QuestId = string.Empty;
        public string Status = "available";
        public int CompletedOnDay;
    }

    [Serializable]
    public sealed class DeepStakeSettlementObjectState
    {
        public string Id = string.Empty;
        public string Type = string.Empty;
        public int X;
        public int Y;
    }

    [Serializable]
    public sealed class DeepStakeSettlementState
    {
        public string Theme = "recovery-farm";
        public int Level = 1;
        public List<DeepStakeSettlementObjectState> Objects = new();
        public List<string> UnlockedObjectTypes = new();
        public List<string> RestoredStructures = new();
    }

    [Serializable]
    public sealed class DeepStakeStoryFlagsState
    {
        public bool MetArchivist;
        public bool MetMechanic;
        public bool EnteredMine;
        public bool HarvestedFirstCrop;
        public bool RepairedLantern;
        public bool CookedFirstMeal;
        public bool RestoredBridge;
        public bool SurveyedNorthReach;
    }

    [Serializable]
    public sealed class DeepStakeAlignmentState
    {
        public int LuminousAffinity;
        public int ShadowAffinity;
        public int ResonanceStability;
        public int CorruptionPressure;
        public int AwakeningClarity;
        public int Compassion;
        public int Domination;
        public int Attunement;
        public string ResonanceBand = "mixed-threshold-band";
        public string AscensionStage = "three-d-world";
    }

    [Serializable]
    public sealed class DeepStakeFactionAffinityState
    {
        public string FactionId = string.Empty;
        public int Value;
    }

    [Serializable]
    public sealed class DeepStakeFactionProfileState
    {
        public string FactionId = string.Empty;
        public string DisplayName = string.Empty;
        public string PublicFace = string.Empty;
        public string HiddenAgenda = string.Empty;
        public string UnlockStage = "three-d-world";
        public string PressureDomain = "land";
    }

    [Serializable]
    public sealed class DeepStakeWorldPressureState
    {
        public string DominantFactionId = "continuum-directorate";
        public int LocalDebtPressure = 20;
        public int LandSeizurePressure = 15;
        public int SupplyChainPressure = 18;
        public int MediaFogPressure = 12;
        public int ResonanceSuppression = 20;
        public int SettlementInfluencePressure = 16;
    }

    [Serializable]
    public sealed class DeepStakeHealthBonusState
    {
        public int StartEnergyBonus;
        public int RecoveryBonus;
        public int CropEfficiencyBonus;
    }

    [Serializable]
    public sealed class DeepStakeSaveData
    {
        public int SchemaVersion = 2;
        public string Slot = "slot-01";
        public string BootMode = "local";
        public int Day = 1;
        public int Minutes = 360;
        public DeepStakePlayerState Player = new();
        public DeepStakeSettlementState Settlement = new();
        public DeepStakeStoryFlagsState StoryFlags = new();
        public DeepStakeAlignmentState Alignment = new();
        public List<DeepStakeFactionAffinityState> FactionAffinities = new();
        public List<DeepStakeFactionProfileState> FactionProfiles = new();
        public DeepStakeWorldPressureState WorldPressure = new();
        public List<DeepStakeQuestState> Quests = new();
        public DeepStakeHealthBonusState HealthBonuses = new();
        public string CurrentZoneId = "recovery-field";
        public string CurrentZoneLabel = "Recovery Field";
        public string LastStatus = "Ready";
    }
}
