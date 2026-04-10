using UnityEngine;

namespace DeepStake.CameraRig
{
    public sealed class QuarterViewCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(7.5f, 8.5f, -7.5f);
        [SerializeField] private float smoothTime = 0.12f;
        [SerializeField] private float lookHeight = 1.35f;
        [SerializeField] private float snapDistance = 18f;

        private Vector3 velocity;

        public void Configure(Transform nextTarget, Vector3 nextOffset)
        {
            target = nextTarget;
            offset = nextOffset;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            var desired = target.position + offset;
            if (Vector3.Distance(transform.position, desired) > snapDistance)
            {
                transform.position = desired;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            transform.LookAt(target.position + Vector3.up * lookHeight);
        }
    }
}
