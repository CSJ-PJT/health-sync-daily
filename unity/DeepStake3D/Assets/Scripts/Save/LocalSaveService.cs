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
            data.Player.Hotbar.Add("hoe");
            data.Player.Hotbar.Add("watering-can");
            data.Player.Hotbar.Add("turnip-seeds");
            data.Player.Inventory.Add(new DeepStakeInventoryEntry { ItemId = "turnip-seeds", Amount = 5 });
            data.Quests.Add(new DeepStakeQuestState { QuestId = "first-harvest", Status = "available" });
            data.Alignment.ResonanceStability = 10;
            data.Alignment.Attunement = 5;
            data.LastStatus = "Local prototype ready";
            return data;
        }
    }
}
