using DeepStake.Core;
using DeepStake.Characters;
using DeepStake.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepStake.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMover3D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2.2f;
        [SerializeField] private float runSpeed = 3.75f;
        [SerializeField] private float acceleration = 6.4f;
        [SerializeField] private float deceleration = 14.5f;
        [SerializeField] private float turnSpeed = 12f;
        [SerializeField] private float gravity = 20f;

        private CharacterController controller;
        private Vector3 velocity;
        private Vector3 planarVelocity;
        private Vector2 input;
        private ArticulatedHumanoidView humanoidView;
        private Vector3 actionFacingDirection = Vector3.zero;
        private float actionFacingTimer;
        private float actionFacingStrength;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            humanoidView = GetComponent<ArticulatedHumanoidView>();
            if (humanoidView == null)
            {
                humanoidView = gameObject.AddComponent<ArticulatedHumanoidView>();
            }
            humanoidView.Configure(ArticulatedHumanoidRole.Player);
        }

        private void Update()
        {
            input = DeepStakeInputBridge.MobileMoveInput;

            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
            }
            input = input.normalized;

            var isRunning = input.sqrMagnitude > 0.62f && DeepStakeInputBridge.SprintHeld;
            var currentMaxSpeed = isRunning ? runSpeed : moveSpeed;
            var desiredMove = new Vector3(input.x, 0f, input.y) * currentMaxSpeed;
            var maxDelta = (desiredMove.sqrMagnitude > planarVelocity.sqrMagnitude ? acceleration : deceleration) * Time.deltaTime;
            planarVelocity = Vector3.MoveTowards(planarVelocity, desiredMove, maxDelta);
            if (input.sqrMagnitude <= 0.001f && planarVelocity.sqrMagnitude < 0.01f)
            {
                planarVelocity = Vector3.zero;
            }

            if (controller != null)
            {
                if (controller.isGrounded && velocity.y < 0f)
                {
                    velocity.y = -1f;
                }

                velocity.y -= gravity * Time.deltaTime;
                controller.Move((planarVelocity + velocity) * Time.deltaTime);
            }

            if (actionFacingTimer > 0f)
            {
                actionFacingTimer -= Time.deltaTime;
            }

            var facing = new Vector3(planarVelocity.x, 0f, planarVelocity.z);
            if (actionFacingTimer > 0.01f && actionFacingDirection.sqrMagnitude > 0.01f)
            {
                var baseFacing = facing.sqrMagnitude > 0.02f ? facing.normalized : transform.forward;
                facing = Vector3.Slerp(baseFacing, actionFacingDirection, actionFacingStrength);
            }
            if (facing.sqrMagnitude > 0.02f)
            {
                var desiredRotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
                var rotationSpeed = actionFacingTimer > 0.01f ? turnSpeed * 1.65f : turnSpeed;
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, rotationSpeed * Time.deltaTime);
            }

            if (humanoidView != null)
            {
                humanoidView.SetMotion(planarVelocity, currentMaxSpeed, isRunning);
            }

            if (DeepStakeGameState.Instance != null)
            {
                var player = DeepStakeGameState.Instance.CurrentSave.Player;
                player.X = transform.position.x;
                player.Y = transform.position.z;
                if (Mathf.Abs(planarVelocity.x) > Mathf.Abs(planarVelocity.z))
                {
                    player.Facing = planarVelocity.x >= 0f ? "right" : "left";
                }
                else if (Mathf.Abs(planarVelocity.z) > 0.01f)
                {
                    player.Facing = planarVelocity.z >= 0f ? "up" : "down";
                }
            }
        }

        public void PlayAction(ArticulatedHumanoidAction action, Vector3 worldTarget)
        {
            if (humanoidView == null)
            {
                return;
            }

            var focus = worldTarget - transform.position;
            focus.y = 0f;
            if (focus.sqrMagnitude > 0.02f)
            {
                actionFacingDirection = focus.normalized;
                actionFacingStrength = GetActionFacingStrength(action);
                actionFacingTimer = GetActionFacingDuration(action);

                var desiredRotation = Quaternion.LookRotation(actionFacingDirection, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, GetActionFacingSnap(action));
            }

            humanoidView.PlayAction(action, worldTarget);
        }

        private static float GetActionFacingDuration(ArticulatedHumanoidAction action)
        {
            return action switch
            {
                ArticulatedHumanoidAction.Attack => 0.68f,
                ArticulatedHumanoidAction.Place => 0.58f,
                ArticulatedHumanoidAction.Talk => 0.5f,
                ArticulatedHumanoidAction.Inspect => 0.46f,
                ArticulatedHumanoidAction.Hit => 0.38f,
                ArticulatedHumanoidAction.Death => 0.9f,
                _ => 0.42f
            };
        }

        private static float GetActionFacingStrength(ArticulatedHumanoidAction action)
        {
            return action switch
            {
                ArticulatedHumanoidAction.Attack => 0.97f,
                ArticulatedHumanoidAction.Place => 0.94f,
                ArticulatedHumanoidAction.Talk => 0.9f,
                ArticulatedHumanoidAction.Inspect => 0.88f,
                _ => 0.82f
            };
        }

        private static float GetActionFacingSnap(ArticulatedHumanoidAction action)
        {
            return action switch
            {
                ArticulatedHumanoidAction.Attack => 0.5f,
                ArticulatedHumanoidAction.Place => 0.42f,
                ArticulatedHumanoidAction.Talk => 0.34f,
                ArticulatedHumanoidAction.Inspect => 0.3f,
                _ => 0.25f
            };
        }
    }
}
