using SpacetimeDB;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private float smoothSpeed = 0.125f;
        [SerializeField] private Vector3 offset;

        private float _fixedZ;
        private Transform _target;
        private Camera _camera;

        private void Awake()
        {
            _fixedZ = transform.position.z;
            _camera = Camera.main;
        }

        private void LateUpdate()
        {
            if (!_target || !_camera) return;

            var desiredPosition = _target.position + offset;
            desiredPosition.z = _fixedZ;

            var halfHeight = _camera.orthographicSize;
            var halfWidth = halfHeight * _camera.aspect;

            //  Clamp bottom to be above death zone
            var bottomY = desiredPosition.y - halfHeight;
            if (bottomY < TerrainHandler.Instance.MinY)
            {
                desiredPosition.y = TerrainHandler.Instance.MinY + halfHeight;
            }

            //  Clamp horizontal view to death zone bounds
            var leftEdge = desiredPosition.x - halfWidth;
            var rightEdge = desiredPosition.x + halfWidth;

            if (leftEdge < TerrainHandler.Instance.MinX)
            {
                desiredPosition.x = TerrainHandler.Instance.MinX + halfWidth;
            }
            else if (rightEdge > TerrainHandler.Instance.MaxX)
            {
                desiredPosition.x = TerrainHandler.Instance.MaxX - halfWidth;
            }

            var smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            // Log.Debug($"CameraFollow Following target: {_target.name} at position: {transform.position}");
        }

        public void SetTarget(Transform newTarget)
        {
            Log.Debug("CameraFollow SetTarget called");
            _target = newTarget;
        }
    }
}