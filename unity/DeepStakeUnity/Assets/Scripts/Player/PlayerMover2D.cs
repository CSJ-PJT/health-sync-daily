using DeepStake.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepStake.Player
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class PlayerMover2D : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 4f;

        private Rigidbody2D rigidBody = null!;
        private Vector2 input;

        private void Awake()
        {
            rigidBody = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
            {
                input = Vector2.zero;
                return;
            }

            input = Vector2.zero;
            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed) input.y -= 1f;
            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed) input.x -= 1f;
            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed) input.x += 1f;
            input = input.normalized;
        }

        private void FixedUpdate()
        {
            rigidBody.linearVelocity = input * moveSpeed;

            if (DeepStakeGameState.Instance == null)
            {
                return;
            }

            var player = DeepStakeGameState.Instance.CurrentSave.Player;
            player.X = transform.position.x;
            player.Y = transform.position.y;
        }
    }
}
