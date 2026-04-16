using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeepStake.UI
{
    public sealed class MobileHoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Action PointerDown;
        public Action PointerUp;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (PointerDown != null)
            {
                PointerDown.Invoke();
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (PointerUp != null)
            {
                PointerUp.Invoke();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (PointerUp != null)
            {
                PointerUp.Invoke();
            }
        }
    }
}
