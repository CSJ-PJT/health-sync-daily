using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace DeepStake.CameraRig
{
    public sealed class QuarterViewCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(7.5f, 8.5f, -7.5f);
        [SerializeField] private float smoothTime = 0.12f;
        [SerializeField] private float lookHeight = 1.35f;
        [SerializeField] private float snapDistance = 18f;
        [SerializeField] private float mobileZoomFactor = 0.2f;
        [SerializeField] private float mobileMinZoomFactor = 0.015f;
        [SerializeField] private float mobileMaxZoomFactor = 0.9f;
        [SerializeField] private float mobilePinchSensitivity = 0.0075f;

        private Vector3 velocity;
        private bool enhancedTouchWasEnabled;

        public void Configure(Transform nextTarget, Vector3 nextOffset)
        {
            target = nextTarget;
            offset = nextOffset;
        }

        private void OnEnable()
        {
            enhancedTouchWasEnabled = EnhancedTouchSupport.enabled;
            if (!EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Enable();
            }
        }

        private void OnDisable()
        {
            if (!enhancedTouchWasEnabled && EnhancedTouchSupport.enabled)
            {
                EnhancedTouchSupport.Disable();
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var effectiveOffset = offset;
            if (Application.isMobilePlatform)
            {
                UpdateMobileZoom();
                var zoomT = Mathf.InverseLerp(mobileMinZoomFactor, mobileMaxZoomFactor, mobileZoomFactor);
                var closeOffset = new Vector3(0.72f, 1.62f, -1.42f);
                var farOffset = new Vector3(offset.x * mobileMaxZoomFactor, offset.y - 0.2f, offset.z * mobileMaxZoomFactor);
                effectiveOffset = Vector3.Lerp(closeOffset, farOffset, zoomT);
            }

            var desired = target.position + effectiveOffset;
            if (Vector3.Distance(transform.position, desired) > snapDistance)
            {
                transform.position = desired;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            var effectiveLookHeight = Application.isMobilePlatform
                ? Mathf.Lerp(1.52f, lookHeight - 0.08f, Mathf.InverseLerp(mobileMinZoomFactor, mobileMaxZoomFactor, mobileZoomFactor))
                : lookHeight;
            transform.LookAt(target.position + Vector3.up * effectiveLookHeight);
        }

        private void UpdateMobileZoom()
        {
            var activeTouches = Touch.activeTouches;
            if (activeTouches.Count < 2)
            {
                return;
            }

            var first = activeTouches[0];
            var second = activeTouches[1];
            var currentDistance = Vector2.Distance(first.screenPosition, second.screenPosition);
            var previousDistance = Vector2.Distance(first.screenPosition - first.delta, second.screenPosition - second.delta);
            var pinchDelta = currentDistance - previousDistance;

            if (Mathf.Abs(pinchDelta) < 0.01f)
            {
                return;
            }

            mobileZoomFactor = Mathf.Clamp(
                mobileZoomFactor - pinchDelta * mobilePinchSensitivity,
                mobileMinZoomFactor,
                mobileMaxZoomFactor);
        }
    }
}
