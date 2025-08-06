using SpacetimeDB;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class CameraFollow : MonoBehaviour
    {
  

        public float smoothSpeed = 0.125f;
        public Vector3 offset;

        private float _fixedZ;
        private Transform _target;

        private Camera _camera;
        private TerrainManager _terrainManager;

        private void Awake()
        {
            _fixedZ = transform.position.z;
            _camera = Camera.main;
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
            if (bottomY < TerrainManager.Instance.MinY)
            {
                desiredPosition.y = TerrainManager.Instance.MinY + halfHeight;
            }

            // --- Clamp horizontal view to death zone bounds ---
            var leftEdge = desiredPosition.x - halfWidth;
            var rightEdge = desiredPosition.x + halfWidth;

            if (leftEdge < TerrainManager.Instance.MinX)
            {
                desiredPosition.x = TerrainManager.Instance.MinX + halfWidth;
            }
            else if (rightEdge > TerrainManager.Instance.MaxX)
            {
                desiredPosition.x = TerrainManager.Instance.MaxX - halfWidth;
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