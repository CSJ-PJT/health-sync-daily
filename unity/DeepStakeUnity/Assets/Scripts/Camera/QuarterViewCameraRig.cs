using UnityEngine;

namespace DeepStake.CameraRig
{
    public sealed class QuarterViewCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(7.5f, 8.5f, -7.5f);
        [SerializeField] private float smoothTime = 0.15f;

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
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            transform.LookAt(target.position + Vector3.up * 1.25f);
        }
    }
}
