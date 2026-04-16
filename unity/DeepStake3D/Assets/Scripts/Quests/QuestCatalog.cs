using System;
using DeepStake.Contracts;
using UnityEngine;

namespace DeepStake.Quests
{
    [Serializable]
    public sealed class DeepStakeQuestDefinition
    {
        public string questId = string.Empty;
        public string title = string.Empty;
        public string objectiveDefault = string.Empty;
        public string objectiveReadNotice = string.Empty;
        public string objectiveMetArchivist = string.Empty;
        public string objectiveReviewedSupply = string.Empty;
        public string objectiveCompleted = string.Empty;
    }

    [Serializable]
    public sealed class DeepStakeQuestCatalogData
    {
        public DeepStakeQuestDefinition[] quests = Array.Empty<DeepStakeQuestDefinition>();
    }

    public static class QuestCatalog
    {
        private static DeepStakeQuestCatalogData cachedCatalog;

        public static void Load(TextAsset questCatalogAsset)
        {
            if (questCatalogAsset == null || string.IsNullOrWhiteSpace(questCatalogAsset.text))
            {
                cachedCatalog = new DeepStakeQuestCatalogData();
                return;
            }

            var parsed = JsonUtility.FromJson<DeepStakeQuestCatalogData>(questCatalogAsset.text);
            cachedCatalog = parsed ?? new DeepStakeQuestCatalogData();
        }

        public static string GetQuestTitle(string questId)
        {
            var quest = FindQuest(questId);
            return quest != null && !string.IsNullOrWhiteSpace(quest.title)
                ? quest.title
                : questId;
        }

        public static string GetCurrentObjective(DeepStakeSaveData save, string questId)
        {
            var quest = FindQuest(questId);
            if (quest == null)
            {
                return "No quest objective available.";
            }

            if (!save.StoryFlags.ReadFieldNoticeBoard)
            {
                return quest.objectiveDefault;
            }

            if (!save.StoryFlags.MetArchivist)
            {
                return quest.objectiveReadNotice;
            }

            if (!save.StoryFlags.ReviewedSupplyCrate)
            {
                return quest.objectiveMetArchivist;
            }

            if (!save.StoryFlags.PlacedRecoveryBeacon)
            {
                return quest.objectiveReviewedSupply;
            }

            return quest.objectiveCompleted;
        }

        public static string GetPrimaryMissionTitle(DeepStakeSaveData save)
        {
            if (IsFirstLoopComplete(save) && !save.StoryFlags.MetFieldHand)
            {
                return GetQuestTitle("observer-followup");
            }

            if (IsFirstLoopComplete(save) && !save.StoryFlags.ReviewedObserverRecord)
            {
                return GetQuestTitle("observer-followup");
            }

            return GetQuestTitle("first-harvest");
        }

        public static string GetPrimaryMissionObjective(DeepStakeSaveData save)
        {
            if (IsFirstLoopComplete(save) && !save.StoryFlags.MetFieldHand)
            {
                return "Speak with Mara Venn by the lane work bench.";
            }

            if (IsFirstLoopComplete(save) && !save.StoryFlags.ReviewedObserverRecord)
            {
                return GetCurrentObjective(save, "observer-followup");
            }

            if (save.StoryFlags.ReviewedObserverRecord)
            {
                return "Observer trace recovered. Prepare the next district lead.";
            }

            return GetCurrentObjective(save, "first-harvest");
        }

        public static string[] GetObjectiveChecklist(DeepStakeSaveData save, string questId)
        {
            var quest = FindQuest(questId);
            if (quest == null)
            {
                return new[] { "No quest data loaded." };
            }

            return new[]
            {
                FormatObjective(save.StoryFlags.ReadFieldNoticeBoard, quest.objectiveDefault),
                FormatObjective(save.StoryFlags.MetArchivist, quest.objectiveReadNotice),
                FormatObjective(save.StoryFlags.ReviewedSupplyCrate, quest.objectiveMetArchivist),
                FormatObjective(save.StoryFlags.PlacedRecoveryBeacon, quest.objectiveReviewedSupply),
                FormatObjective(
                    save.StoryFlags.PlacedRecoveryBeacon && save.Settlement.Objects.Exists(item => item.Type == "supply-relay"),
                    "Anchor the supply relay and stabilize the field lane."),
                FormatObjective(save.StoryFlags.MetFieldHand, "Speak with Mara Venn at the lane work bench and confirm the next handoff."),
                FormatObjective(save.StoryFlags.ReviewedObserverRecord, "Inspect the observer record case and secure the next district lead.")
            };
        }

        private static DeepStakeQuestDefinition FindQuest(string questId)
        {
            if (cachedCatalog == null || cachedCatalog.quests == null)
            {
                return null;
            }

            for (var index = 0; index < cachedCatalog.quests.Length; index++)
            {
                if (cachedCatalog.quests[index].questId == questId)
                {
                    return cachedCatalog.quests[index];
                }
            }

            return null;
        }

        private static string FormatObjective(bool completed, string text)
        {
            return (completed ? "[x] " : "[ ] ") + text;
        }

        private static bool IsFirstLoopComplete(DeepStakeSaveData save)
        {
            return save.StoryFlags.ReadFieldNoticeBoard &&
                   save.StoryFlags.MetArchivist &&
                   save.StoryFlags.ReviewedSupplyCrate &&
                   save.StoryFlags.PlacedRecoveryBeacon;
        }
    }
}
