using UnityEngine;

namespace masks.client.Scripts
{
    public class ProjectileController : MonoBehaviour
    {
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (!_mainCamera) 
                return;
            
            var screenPoint = _mainCamera.WorldToViewportPoint(transform.position);
            if (screenPoint.x < 0 || screenPoint.x > 1 || screenPoint.y < 0 || screenPoint.y > 1)
            {
                Destroy(gameObject);
            }
        }
    }
}
