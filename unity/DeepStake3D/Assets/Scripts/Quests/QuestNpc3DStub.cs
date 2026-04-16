using DeepStake.Core;
using DeepStake.Characters;
using DeepStake.Player;
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
        [SerializeField] private float talkRange = 5.5f;
        [SerializeField] private float awarenessRange = 7.5f;

        private ArticulatedHumanoidView humanoidView;
        private PlayerMover3D playerMover;

        public void Configure(string nextNpcId, string nextQuestId, string nextDialogue)
        {
            npcId = nextNpcId;
            questId = nextQuestId;
            dialogue = nextDialogue;
            displayName = nextNpcId;
            if (humanoidView != null)
            {
                humanoidView.Configure(npcId == "field-hand" ? ArticulatedHumanoidRole.FieldWorker : ArticulatedHumanoidRole.Archivist);
            }
        }

        public void Configure(string nextNpcId, string nextDisplayName, string nextQuestId, string nextDialogue)
        {
            npcId = nextNpcId;
            displayName = nextDisplayName;
            questId = nextQuestId;
            dialogue = nextDialogue;
            if (humanoidView != null)
            {
                humanoidView.Configure(npcId == "field-hand" ? ArticulatedHumanoidRole.FieldWorker : ArticulatedHumanoidRole.Archivist);
            }
        }

        private void Awake()
        {
            humanoidView = GetComponent<ArticulatedHumanoidView>();
            if (humanoidView == null)
            {
                humanoidView = gameObject.AddComponent<ArticulatedHumanoidView>();
            }
            humanoidView.Configure(npcId == "field-hand" ? ArticulatedHumanoidRole.FieldWorker : ArticulatedHumanoidRole.Archivist);
        }

        private void Update()
        {
            if (playerMover == null)
            {
                playerMover = FindFirstObjectByType<PlayerMover3D>();
            }

            if (humanoidView == null)
            {
                return;
            }

            humanoidView.SetMotion(Vector3.zero, 1f);
            if (playerMover == null)
            {
                humanoidView.ClearAttention();
                return;
            }

            var distance = GetDistance(playerMover.transform);
            if (distance <= awarenessRange)
            {
                humanoidView.SetAttentionTarget(playerMover.transform.position, Mathf.InverseLerp(awarenessRange, talkRange, distance));
            }
            else
            {
                humanoidView.ClearAttention();
            }
        }

        public bool CanTalk(Transform actor)
        {
            return GetDistance(actor) <= talkRange;
        }

        public float GetDistance(Transform actor)
        {
            if (actor == null)
            {
                return float.MaxValue;
            }

            var actorFlat = new Vector3(actor.position.x, transform.position.y, actor.position.z);
            return Vector3.Distance(actorFlat, transform.position);
        }

        public string GetPrompt()
        {
            return "Q Talk  " + displayName;
        }

        public string GetLabel()
        {
            return displayName;
        }

        public void Talk()
        {
            if (DeepStakeGameState.Instance == null)
            {
                return;
            }

            var save = DeepStakeGameState.Instance.CurrentSave;
            var wasMetFieldHand = save.StoryFlags.MetFieldHand;
            save.StoryFlags.MetArchivist |= npcId == "archivist";
            save.StoryFlags.MetFieldHand |= npcId == "field-hand";
            save.StoryFlags.MetMechanic |= npcId == "mechanic";
            save.StoryFlags.LearnedDirectoratePressure |= npcId == "archivist";

            var resolvedDialogue = ResolveDialogue(save, wasMetFieldHand);

            foreach (var quest in save.Quests)
            {
                if (quest.QuestId != questId)
                {
                    continue;
                }

                if (quest.Status != "completed")
                {
                    quest.Status = save.StoryFlags.ReviewedSupplyCrate ? "ready-for-beacon" : "met-archivist";
                    save.Alignment.AwakeningClarity += 1;
                    save.Alignment.Compassion += 1;
                }
                break;
            }

            save.LastStatus = displayName + ": " + resolvedDialogue;
            DeepStakeGameState.Instance.UpdateStatus(save.LastStatus);
            if (humanoidView != null && playerMover != null)
            {
                humanoidView.PlayAction(ArticulatedHumanoidAction.Talk, playerMover.transform.position);
            }
        }

        private string ResolveDialogue(DeepStake.Contracts.DeepStakeSaveData save, bool wasMetFieldHand)
        {
            if (npcId != "archivist")
            {
                if (npcId == "field-hand")
                {
                    if (!save.StoryFlags.PlacedRecoveryBeacon)
                    {
                        return "I can clear the lane once the beacon is steady. Get the anchor up first.";
                    }

                    if (!wasMetFieldHand)
                    {
                        return "Good. The beacon gave us one steady line again. I marked an observer case by the archive fence. Check it before the next lane goes dark.";
                    }

                    return "The lane is breathing again. We can move supplies, but only if the record trail holds.";
                }

                return dialogue;
            }

            if (!save.StoryFlags.ReadFieldNoticeBoard)
            {
                return "Read the field notice first. The crop order and the land transfer should not share the same seal, but they do.";
            }

            if (!save.StoryFlags.ReviewedSupplyCrate)
            {
                return "Check the delayed supply crates next. The route sheets were cut and stamped twice.";
            }

            if (!save.StoryFlags.PlacedRecoveryBeacon)
            {
                return "That is enough proof for now. Anchor one recovery beacon before the lane closes again.";
            }

            return "Good. The beacon held. We have a stable record, a supply trace, and one anchored recovery point.";
        }
    }
}
