using SpacetimeDB;
using UnityEngine;

namespace masks.client.Scripts
{
    public class CameraFollow : MonoBehaviour
    {
        public float smoothSpeed = 0.125f;
        public Vector3 offset;

        private float _fixedZ;
        private Transform _target;

        private void Awake()
        {
            _fixedZ = transform.position.z;
        }
        
        private void LateUpdate()
        {
            if (!_target) return;

            Vector3 desiredPosition = _target.position + offset;
            desiredPosition.z = _fixedZ;

            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
            
            Log.Debug($"CameraFollow Following target: {_target.name} at position: {transform.position}");
        }

        public void SetTarget(Transform newTarget)
        {
            Log.Debug("CameraFollow SetTarget called");
            _target = newTarget;
        }
    }
}