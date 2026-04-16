using System.IO;
using DeepStake.Contracts;
using UnityEngine;

namespace DeepStake.Save
{
    public static class LocalSaveService
    {
        private const string FileName = "deepstake-slot-01.json";

        public static string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, FileName);
        }

        public static bool Exists()
        {
            return File.Exists(GetSavePath());
        }

        public static DeepStakeSaveData LoadOrCreate()
        {
            var path = GetSavePath();
            if (!File.Exists(path))
            {
                return CreateDefault();
            }

            var json = File.ReadAllText(path);
            var parsed = JsonUtility.FromJson<DeepStakeSaveData>(json);
            return parsed != null ? parsed : CreateDefault();
        }

        public static void Save(DeepStakeSaveData data)
        {
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetSavePath(), json);
        }

        public static DeepStakeSaveData CreateDefault()
        {
            var data = new DeepStakeSaveData();
            data.CurrentZoneId = "recovery-field";
            data.CurrentZoneLabel = "Longest Dawn | Recovery Field";
            data.Player.MapId = "recovery-field";
            data.FactionAffinities.Add(new DeepStakeFactionAffinityState
            {
                FactionId = "continuum-directorate",
                Value = -20
            });
            data.FactionAffinities.Add(new DeepStakeFactionAffinityState
            {
                FactionId = "deep-archive",
                Value = 8
            });
            data.FactionAffinities.Add(new DeepStakeFactionAffinityState
            {
                FactionId = "dawnkeepers",
                Value = 5
            });
            data.FactionProfiles.Add(new DeepStakeFactionProfileState
            {
                FactionId = "continuum-directorate",
                DisplayName = "Continuum Directorate",
                PublicFace = "Investment, relief, research, and infrastructure coordination trusts",
                HiddenAgenda = "Expands control through debt, land consolidation, supply disruption, and resonance suppression",
                UnlockStage = "three-d-world",
                PressureDomain = "logistics"
            });
            data.Player.Hotbar.Add("hoe");
            data.Player.Hotbar.Add("watering-can");
            data.Player.Hotbar.Add("turnip-seeds");
            data.Player.Inventory.Add(new DeepStakeInventoryEntry { ItemId = "turnip-seeds", Amount = 5 });
            data.Quests.Add(new DeepStakeQuestState { QuestId = "first-harvest", Status = "inspect-notice" });
            data.Alignment.ResonanceStability = 10;
            data.Alignment.Attunement = 5;
            data.WorldPressure.DominantFactionId = "continuum-directorate";
            data.WorldPressure.LocalDebtPressure = 24;
            data.WorldPressure.LandSeizurePressure = 18;
            data.WorldPressure.SupplyChainPressure = 20;
            data.WorldPressure.MediaFogPressure = 14;
            data.WorldPressure.ResonanceSuppression = 22;
            data.WorldPressure.SettlementInfluencePressure = 19;
            data.ActivePressureHint = "Continuum Directorate filings are beginning to reappear in local debt and land records.";
            data.LastStatus = "Local recovery field ready";
            data.Settlement.RestoredStructures.Add("field-edge-access");
            return data;
        }
    }
}
