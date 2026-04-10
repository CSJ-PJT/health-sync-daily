using DeepStake.Core;
using UnityEngine;

namespace DeepStake.Quests
{
    public sealed class QuestNpc3DStub : MonoBehaviour
    {
        [SerializeField] private string npcId = "archivist";
        [SerializeField] private string displayName = "Archivist";
        [SerializeField] [TextArea] private string dialogue =
            "The archive listens for the first proof that this ground can recover.";
        [SerializeField] private string questId = "first-harvest";
        [SerializeField] private float talkRange = 2.5f;

        public void Configure(string nextNpcId, string nextQuestId, string nextDialogue)
        {
            npcId = nextNpcId;
            questId = nextQuestId;
            dialogue = nextDialogue;
            displayName = nextNpcId;
        }

        public void Configure(string nextNpcId, string nextDisplayName, string nextQuestId, string nextDialogue)
        {
            npcId = nextNpcId;
            displayName = nextDisplayName;
            questId = nextQuestId;
            dialogue = nextDialogue;
        }

        public bool CanTalk(Transform actor)
        {
            if (actor == null)
            {
                return false;
            }

            var actorFlat = new Vector3(actor.position.x, transform.position.y, actor.position.z);
            return Vector3.Distance(actorFlat, transform.position) <= talkRange;
        }

        public string GetPrompt()
        {
            return "Q Talk  " + displayName;
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

                if (quest.Status != "completed")
                {
                    quest.Status = "completed";
                    quest.CompletedOnDay = save.Day;
                    save.Alignment.AwakeningClarity += 1;
                    save.Alignment.Compassion += 1;
                }
                break;
            }

            save.LastStatus = displayName + ": " + dialogue;
            DeepStakeGameState.Instance.UpdateStatus(save.LastStatus);
        }
    }
}
