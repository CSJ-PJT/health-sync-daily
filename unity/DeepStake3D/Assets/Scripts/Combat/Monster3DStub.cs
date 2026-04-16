using DeepStake.Characters;
using DeepStake.Core;
using UnityEngine;

namespace DeepStake.Combat
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class Monster3DStub : MonoBehaviour
    {
        [SerializeField] private float awarenessRange = 8f;
        [SerializeField] private float attackRange = 1.65f;
        [SerializeField] private float moveSpeed = 2.6f;
        [SerializeField] private float turnSpeed = 8f;
        [SerializeField] private float attackCooldown = 1.2f;
        [SerializeField] private int maxHealth = 4;

        private CharacterController controller;
        private ArticulatedHumanoidView humanoidView;
        private Transform player;
        private float attackTimer;
        private int currentHealth;
        private bool defeated;

        public bool IsDefeated => defeated;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            humanoidView = GetComponent<ArticulatedHumanoidView>();
            if (humanoidView == null)
            {
                humanoidView = gameObject.AddComponent<ArticulatedHumanoidView>();
            }

            humanoidView.Configure(ArticulatedHumanoidRole.Monster);
            currentHealth = maxHealth;
        }

        public void Configure(Transform playerTransform)
        {
            player = playerTransform;
        }

        public void ResetState(bool alreadyDefeated)
        {
            defeated = alreadyDefeated;
            currentHealth = maxHealth;
            gameObject.SetActive(!alreadyDefeated);
        }

        private void Update()
        {
            if (defeated || player == null)
            {
                if (humanoidView != null)
                {
                    humanoidView.SetMotion(Vector3.zero, moveSpeed);
                }
                return;
            }

            if (attackTimer > 0f)
            {
                attackTimer -= Time.deltaTime;
            }

            var toPlayer = player.position - transform.position;
            toPlayer.y = 0f;
            var distance = toPlayer.magnitude;

            if (distance > awarenessRange)
            {
                if (humanoidView != null)
                {
                    humanoidView.SetMotion(Vector3.zero, moveSpeed);
                }
                return;
            }

            if (toPlayer.sqrMagnitude > 0.001f)
            {
                var desiredRotation = Quaternion.LookRotation(toPlayer.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, turnSpeed * Time.deltaTime);
                humanoidView.SetAttentionTarget(player.position, 1f);
            }

            if (distance > attackRange)
            {
                var motion = toPlayer.normalized * moveSpeed;
                controller.Move(motion * Time.deltaTime);
                humanoidView.SetMotion(motion, moveSpeed);
                return;
            }

            humanoidView.SetMotion(Vector3.zero, moveSpeed);
            if (attackTimer <= 0f)
            {
                attackTimer = attackCooldown;
                humanoidView.PlayAction(ArticulatedHumanoidAction.Attack, player.position);
                if (DeepStakeGameState.Instance != null)
                {
                    var save = DeepStakeGameState.Instance.CurrentSave;
                    save.Player.Energy = Mathf.Max(0, save.Player.Energy - 6);
                    DeepStakeGameState.Instance.UpdateStatus("The husk lashes out. Hold ground and strike back.");
                }
            }
        }

        public bool CanBeHitFrom(Vector3 attackerPosition, float range)
        {
            if (defeated)
            {
                return false;
            }

            var flat = attackerPosition - transform.position;
            flat.y = 0f;
            return flat.magnitude <= range;
        }

        public void ReceiveHit(Vector3 attackerPosition)
        {
            if (defeated)
            {
                return;
            }

            currentHealth = Mathf.Max(0, currentHealth - 1);
            humanoidView.PlayAction(ArticulatedHumanoidAction.Hit, attackerPosition);

            if (currentHealth > 0)
            {
                if (DeepStakeGameState.Instance != null)
                {
                    DeepStakeGameState.Instance.UpdateStatus("Hit confirmed. The husk staggers.");
                }
                return;
            }

            defeated = true;
            if (DeepStakeGameState.Instance != null)
            {
                DeepStakeGameState.Instance.CurrentSave.StoryFlags.DefeatedFirstMonster = true;
                DeepStakeGameState.Instance.UpdateStatus("The first husk drops. Longest Dawn holds for now.");
            }

            gameObject.SetActive(false);
        }
    }
}
