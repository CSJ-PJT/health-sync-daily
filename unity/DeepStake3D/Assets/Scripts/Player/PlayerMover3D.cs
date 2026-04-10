using DeepStake.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepStake.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMover3D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5.5f;
        [SerializeField] private float acceleration = 14f;
        [SerializeField] private float turnSpeed = 12f;
        [SerializeField] private float gravity = 20f;

        private CharacterController controller;
        private Vector3 velocity;
        private Vector3 planarVelocity;
        private Vector2 input;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                input = Vector2.zero;
            }
            else
            {
                input = Vector2.zero;
                if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
                if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
                if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
                if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
                input = input.normalized;
            }

            var desiredMove = new Vector3(input.x, 0f, input.y) * moveSpeed;
            planarVelocity = Vector3.Lerp(planarVelocity, desiredMove, acceleration * Time.deltaTime);

            if (controller != null)
            {
                if (controller.isGrounded && velocity.y < 0f)
                {
                    velocity.y = -1f;
                }

                velocity.y -= gravity * Time.deltaTime;
                controller.Move((planarVelocity + velocity) * Time.deltaTime);
            }

            var facing = new Vector3(planarVelocity.x, 0f, planarVelocity.z);
            if (facing.sqrMagnitude > 0.02f)
            {
                var desiredRotation = Quaternion.LookRotation(facing.normalized, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, desiredRotation, turnSpeed * Time.deltaTime);
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
    }
}
