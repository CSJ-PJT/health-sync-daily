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
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float acceleration = 14f;
        [SerializeField] private float deceleration = 18f;
        [SerializeField] private float turnSpeed = 12f;
        [SerializeField] private float gravity = 20f;

        private CharacterController controller;
        private Vector3 velocity;
        private Vector3 planarVelocity;
        private Vector2 input;
        private ArticulatedHumanoidView humanoidView;
        private Vector3 actionFacingDirection = Vector3.zero;
        private float actionFacingTimer;

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

            var desiredMove = new Vector3(input.x, 0f, input.y) * moveSpeed;
            var maxDelta = (desiredMove.sqrMagnitude > planarVelocity.sqrMagnitude ? acceleration : deceleration) * Time.deltaTime;
            planarVelocity = Vector3.MoveTowards(planarVelocity, desiredMove, maxDelta);

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
                facing = Vector3.Slerp(facing.sqrMagnitude > 0.02f ? facing.normalized : transform.forward, actionFacingDirection, 0.72f);
            }
            if (facing.sqrMagnitude > 0.02f)
            {
                var desiredRotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, turnSpeed * Time.deltaTime);
            }

            if (humanoidView != null)
            {
                humanoidView.SetMotion(planarVelocity, moveSpeed);
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
                actionFacingTimer = action == ArticulatedHumanoidAction.Place ? 0.42f : 0.3f;
            }

            humanoidView.PlayAction(action, worldTarget);
        }
    }
}
