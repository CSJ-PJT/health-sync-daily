using UnityEngine;
using UnityEngine.InputSystem;
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
        [SerializeField] private float mobileZoomFactor = 0.82f;
        [SerializeField] private float mobileMinZoomFactor = 0.16f;
        [SerializeField] private float mobileMaxZoomFactor = 1.18f;
        [SerializeField] private float mobilePinchSensitivity = 0.011f;
        [SerializeField] private float editorMouseWheelSensitivity = 0.22f;

        private Vector3 velocity;
        private bool enhancedTouchWasEnabled;

        private void Awake()
        {
            var cameraComponent = GetComponent<Camera>();
            if (cameraComponent != null)
            {
                cameraComponent.clearFlags = CameraClearFlags.SolidColor;
                cameraComponent.backgroundColor = new Color(0.55f, 0.68f, 0.78f);
                cameraComponent.fieldOfView = Mathf.Max(cameraComponent.fieldOfView, 62f);
            }

            RenderSettings.ambientLight = new Color(0.62f, 0.66f, 0.68f);
            RenderSettings.fog = true;
            RenderSettings.fogColor = new Color(0.62f, 0.72f, 0.78f);
            RenderSettings.fogDensity = 0.009f;
        }

        public void Configure(Transform nextTarget, Vector3 nextOffset)
        {
            target = nextTarget;
            offset = nextOffset;
            mobileZoomFactor = Mathf.Clamp(Mathf.Max(mobileZoomFactor, 0.82f), mobileMinZoomFactor, mobileMaxZoomFactor);
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
            var supportsZoom = Application.isMobilePlatform || Application.isEditor;
            if (supportsZoom)
            {
                UpdateZoomInput();
                var zoomT = Mathf.InverseLerp(mobileMinZoomFactor, mobileMaxZoomFactor, mobileZoomFactor);
                var closeOffset = (-target.forward * 3.35f) + (target.right * -0.85f) + (Vector3.up * 1.85f);
                var farOffset = new Vector3(offset.x * mobileMaxZoomFactor, offset.y + 0.7f, offset.z * mobileMaxZoomFactor);
                effectiveOffset = Vector3.Lerp(closeOffset, farOffset, zoomT);
            }

            var desired = target.position + effectiveOffset;
            if (Vector3.Distance(transform.position, desired) > snapDistance)
            {
                transform.position = desired;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            var effectiveLookHeight = supportsZoom
                ? Mathf.Lerp(0.82f, lookHeight + 0.12f, Mathf.InverseLerp(mobileMinZoomFactor, mobileMaxZoomFactor, mobileZoomFactor))
                : lookHeight;
            transform.LookAt(target.position + Vector3.up * effectiveLookHeight);
        }

        private void UpdateZoomInput()
        {
            if (Application.isEditor)
            {
                var mouse = Mouse.current;
                var scroll = mouse != null ? mouse.scroll.ReadValue().y / 120f : 0f;
                if (Mathf.Abs(scroll) > 0.001f)
                {
                    mobileZoomFactor = Mathf.Clamp(
                        mobileZoomFactor - scroll * editorMouseWheelSensitivity,
                        mobileMinZoomFactor,
                        mobileMaxZoomFactor);
                }

                var keyboard = Keyboard.current;
                if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                {
                mobileZoomFactor = 0.82f;
                }
            }

            if (!Application.isMobilePlatform)
            {
                return;
            }

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
