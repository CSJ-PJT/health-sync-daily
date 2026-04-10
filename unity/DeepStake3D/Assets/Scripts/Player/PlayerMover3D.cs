using DeepStake.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepStake.Player
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerMover3D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float gravity = 20f;

        private CharacterController controller;
        private Vector3 velocity;
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

            var move = new Vector3(input.x, 0f, input.y) * moveSpeed;

            if (controller != null)
            {
                if (controller.isGrounded && velocity.y < 0f)
                {
                    velocity.y = -1f;
                }

                velocity.y -= gravity * Time.deltaTime;
                controller.Move((move + velocity) * Time.deltaTime);
            }

            if (DeepStakeGameState.Instance != null)
            {
                var player = DeepStakeGameState.Instance.CurrentSave.Player;
                player.X = transform.position.x;
                player.Y = transform.position.z;
            }
        }
    }
}
