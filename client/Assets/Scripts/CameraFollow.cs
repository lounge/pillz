using SpacetimeDB;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class CameraFollow : MonoBehaviour
    {

        [Header("Clamp Settings")] [SerializeField]
        private Collider2D deathZone;

        public float smoothSpeed = 0.125f;
        public Vector3 offset;

        private float _fixedZ;
        private Transform _target;
        private float _minY;
        private float _minX;
        private float _maxX;

        private Camera _camera;

        private void Awake()
        {
            _fixedZ = transform.position.z;
            _camera = Camera.main;

            if (deathZone)
            {
                var bounds = deathZone.bounds;
                _minY = bounds.max.y; 
                _minX = bounds.min.x;
                _maxX = bounds.max.x;
            }
        }

        private void LateUpdate()
        {
            if (!_target || !_camera) return;

            Vector3 desiredPosition = _target.position + offset;
            desiredPosition.z = _fixedZ;

            var halfHeight = _camera.orthographicSize;
            var halfWidth = halfHeight * _camera.aspect;

            // --- Clamp bottom to be above death zone ---
            var bottomY = desiredPosition.y - halfHeight;
            if (bottomY < _minY)
            {
                desiredPosition.y = _minY + halfHeight;
            }

            // --- Clamp horizontal view to death zone bounds ---
            var leftEdge = desiredPosition.x - halfWidth;
            var rightEdge = desiredPosition.x + halfWidth;

            if (leftEdge < _minX)
            {
                desiredPosition.x = _minX + halfWidth;
            }
            else if (rightEdge > _maxX)
            {
                desiredPosition.x = _maxX - halfWidth;
            }

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