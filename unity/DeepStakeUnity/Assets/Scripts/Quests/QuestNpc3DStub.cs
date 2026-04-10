using DeepStake.Core;
using UnityEngine;

namespace DeepStake.Quests
{
    public sealed class QuestNpc3DStub : MonoBehaviour
    {
        [SerializeField] private string npcId = "archivist";
        [SerializeField] [TextArea] private string dialogue =
            "The archive listens for the first proof that this ground can recover.";
        [SerializeField] private string questId = "first-harvest";

        public void Configure(string nextNpcId, string nextQuestId, string nextDialogue)
        {
            npcId = nextNpcId;
            questId = nextQuestId;
            dialogue = nextDialogue;
        }

        public void Talk()
        {
            if (DeepStakeGameState.Instance == null)
            {
                return;
            }

            var save = DeepStakeGameState.Instance.CurrentSave;
            save.StoryFlags.MetArchivist |= npcId == "archivist";
            save.StoryFlags.MetMechanic |= npcId == "mechanic";

            foreach (var quest in save.Quests)
            {
                if (quest.QuestId != questId)
                {
                    continue;
                }

                quest.Status = "completed";
                quest.CompletedOnDay = save.Day;
                break;
            }

            save.LastStatus = npcId + ": " + dialogue;
            DeepStakeGameState.Instance.UpdateStatus(save.LastStatus);
        }
    }
}
