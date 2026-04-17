using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepStake.Input
{
    public static class DeepStakeInputBridge
    {
        private static Vector2 mobileMoveInput;
        private static bool interactPressed;
        private static bool talkPressed;
        private static bool placePressed;
        private static bool attackPressed;
        private static bool sprintHeld;
        private static bool savePressed;
        private static bool reloadPressed;
        private static bool journalPressed;
        private static string inputModeLabel = "keyboard";

        public static Vector2 MobileMoveInput => mobileMoveInput;
        public static string InputModeLabel => inputModeLabel;
        public static bool SprintHeld => sprintHeld;

        public static void PollHardware()
        {
            inputModeLabel = "keyboard";
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                sprintHeld = keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed;

                if (keyboard.eKey.wasPressedThisFrame)
                {
                    interactPressed = true;
                }

                if (keyboard.qKey.wasPressedThisFrame)
                {
                    talkPressed = true;
                }

                if (keyboard.bKey.wasPressedThisFrame)
                {
                    placePressed = true;
                }

                if (keyboard.fKey.wasPressedThisFrame || keyboard.spaceKey.wasPressedThisFrame)
                {
                    attackPressed = true;
                }

                if (keyboard.f5Key.wasPressedThisFrame)
                {
                    savePressed = true;
                }

                if (keyboard.f9Key.wasPressedThisFrame)
                {
                    reloadPressed = true;
                }

                if (keyboard.jKey.wasPressedThisFrame)
                {
                    journalPressed = true;
                }
            }
            else
            {
                sprintHeld = false;
            }
        }

        public static void SetMobileMove(Vector2 nextMove)
        {
            mobileMoveInput = Vector2.ClampMagnitude(nextMove, 1f);
            inputModeLabel = "mobile";
        }

        public static void ClearMobileMove()
        {
            mobileMoveInput = Vector2.zero;
        }

        public static void PressInteract()
        {
            interactPressed = true;
            inputModeLabel = "mobile";
        }

        public static void PressTalk()
        {
            talkPressed = true;
            inputModeLabel = "mobile";
        }

        public static void PressPlace()
        {
            placePressed = true;
            inputModeLabel = "mobile";
        }

        public static void PressAttack()
        {
            attackPressed = true;
            inputModeLabel = "mobile";
        }

        public static void PressSave()
        {
            savePressed = true;
            inputModeLabel = "mobile";
        }

        public static void PressReload()
        {
            reloadPressed = true;
            inputModeLabel = "mobile";
        }

        public static void PressJournal()
        {
            journalPressed = true;
            inputModeLabel = "mobile";
        }

        public static bool ConsumeInteract()
        {
            if (!interactPressed)
            {
                return false;
            }

            interactPressed = false;
            return true;
        }

        public static bool ConsumeTalk()
        {
            if (!talkPressed)
            {
                return false;
            }

            talkPressed = false;
            return true;
        }

        public static bool ConsumePlace()
        {
            if (!placePressed)
            {
                return false;
            }

            placePressed = false;
            return true;
        }

        public static bool ConsumeAttack()
        {
            if (!attackPressed)
            {
                return false;
            }

            attackPressed = false;
            return true;
        }

        public static bool ConsumeSave()
        {
            if (!savePressed)
            {
                return false;
            }

            savePressed = false;
            return true;
        }

        public static bool ConsumeReload()
        {
            if (!reloadPressed)
            {
                return false;
            }

            reloadPressed = false;
            return true;
        }

        public static bool ConsumeJournal()
        {
            if (!journalPressed)
            {
                return false;
            }

            journalPressed = false;
            return true;
        }
    }
}
