using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using DeepStake.Rendering;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace DeepStake.CameraRig
{
    public sealed class QuarterViewCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(5.6f, 5.95f, -5.6f);
        [SerializeField] private float smoothTime = 0.1f;
        [SerializeField] private float lookHeight = 1.16f;
        [SerializeField] private float snapDistance = 18f;
        [SerializeField] private float mobileZoomFactor = 0.46f;
        [SerializeField] private float mobileMinZoomFactor = 0.18f;
        [SerializeField] private float mobileMaxZoomFactor = 1.04f;
        [SerializeField] private float mobilePinchSensitivity = 0.018f;
        [SerializeField] private float mobileOrbitSensitivity = 0.08f;
        [SerializeField] private float editorMouseWheelSensitivity = 0.38f;
        [SerializeField] private float editorOrbitSensitivity = 90f;

        private Vector3 velocity;
        private bool enhancedTouchWasEnabled;
        private float orbitYaw;

        private void Awake()
        {
            var cameraComponent = GetComponent<Camera>();
            if (cameraComponent != null)
            {
                cameraComponent.clearFlags = CameraClearFlags.SolidColor;
                cameraComponent.backgroundColor = new Color(0.66f, 0.71f, 0.72f);
                cameraComponent.fieldOfView = Mathf.Clamp(cameraComponent.fieldOfView, 49f, 56f);
            }

            DeepStakePbrEnvironmentPipeline.ApplyLightingProfile();
        }

        public void Configure(Transform nextTarget, Vector3 nextOffset)
        {
            target = nextTarget;
            offset = NormalizeReadableOffset(nextOffset);
            mobileZoomFactor = Mathf.Clamp(mobileZoomFactor, mobileMinZoomFactor, mobileMaxZoomFactor);
        }

        private static Vector3 NormalizeReadableOffset(Vector3 requestedOffset)
        {
            var planar = new Vector3(requestedOffset.x, 0f, requestedOffset.z);
            if (planar.sqrMagnitude < 0.01f)
            {
                planar = new Vector3(1f, 0f, -1f);
            }

            var clampedDistance = Mathf.Clamp(planar.magnitude, 4.95f, 5.95f);
            var readableHeight = Mathf.Clamp(requestedOffset.y, 4.2f, 5.2f);
            var normalized = planar.normalized * clampedDistance;
            return new Vector3(normalized.x, readableHeight, normalized.z);
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
                var fixedQuarterDirection = new Vector3(offset.x, 0f, offset.z);
                if (fixedQuarterDirection.sqrMagnitude < 0.01f)
                {
                    fixedQuarterDirection = new Vector3(1f, 0f, -1f);
                }

                var orbitRotation = Quaternion.Euler(0f, orbitYaw, 0f);
                fixedQuarterDirection = orbitRotation * fixedQuarterDirection.normalized;
                var closeOffset = fixedQuarterDirection * 2.9f + Vector3.up * 1.8f;
                var farOffset = orbitRotation * new Vector3(offset.x * mobileMaxZoomFactor, offset.y + 0.35f, offset.z * mobileMaxZoomFactor);
                effectiveOffset = Vector3.Lerp(closeOffset, farOffset, zoomT);
            }

            var desired = target.position + effectiveOffset;
            if (Vector3.Distance(transform.position, desired) > snapDistance)
            {
                transform.position = desired;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            var effectiveLookHeight = supportsZoom
                ? Mathf.Lerp(0.94f, lookHeight + 0.02f, Mathf.InverseLerp(mobileMinZoomFactor, mobileMaxZoomFactor, mobileZoomFactor))
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
                    mobileZoomFactor = 0.62f;
                    orbitYaw = 0f;
                }

                if (mouse != null && mouse.rightButton.isPressed)
                {
                    orbitYaw += mouse.delta.ReadValue().x * editorOrbitSensitivity * Time.unscaledDeltaTime;
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
            var averageDelta = (first.delta + second.delta) * 0.5f;
            var isOrbitGesture = Mathf.Abs(averageDelta.x) > Mathf.Abs(pinchDelta) * 0.45f;

            if (isOrbitGesture && Mathf.Abs(averageDelta.x) > 0.08f)
            {
                orbitYaw += averageDelta.x * mobileOrbitSensitivity;
            }

            if (isOrbitGesture || Mathf.Abs(pinchDelta) < 0.01f)
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
