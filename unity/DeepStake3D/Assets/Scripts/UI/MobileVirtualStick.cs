using UnityEngine;
using UnityEngine.EventSystems;

namespace DeepStake.UI
{
    public sealed class MobileVirtualStick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public RectTransform StickRoot;
        public RectTransform Handle;
        public float Radius = 78f;

        public void OnPointerDown(PointerEventData eventData)
        {
            UpdateStick(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            UpdateStick(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (Handle != null)
            {
                Handle.anchoredPosition = Vector2.zero;
            }

            DeepStake.Input.DeepStakeInputBridge.ClearMobileMove();
        }

        private void UpdateStick(PointerEventData eventData)
        {
            if (StickRoot == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    StickRoot,
                    eventData.position,
                    eventData.pressEventCamera,
                    out var localPoint))
            {
                return;
            }

            var move = Vector2.ClampMagnitude(localPoint / Mathf.Max(Radius, 1f), 1f);
            if (Handle != null)
            {
                Handle.anchoredPosition = move * Radius;
            }

            DeepStake.Input.DeepStakeInputBridge.SetMobileMove(move);
        }
    }
}
