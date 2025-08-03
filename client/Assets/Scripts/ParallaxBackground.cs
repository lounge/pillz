using UnityEngine;
using UnityEngine.UI;

namespace masks.client.Scripts
{
    public class ParallaxBackground : MonoBehaviour
    {
        [Tooltip("Constant scroll over time")]
        public Vector2 scrollSpeed = new(0.01f, 0.01f);

        [Tooltip("Multiplier for camera-based parallax movement. Lower = more depth.")]
        public Vector2 parallaxMultiplier = new(0.05f, 0.05f);

        private RawImage _rawImage;
        private Vector2 _uvOffset;

        private Transform _cameraTransform;
        private Vector3 _lastCameraPosition;

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
            _cameraTransform = Camera.main!.transform;
            _lastCameraPosition = _cameraTransform.position;
        }

        private void Update()
        {
            _uvOffset += scrollSpeed * Time.deltaTime;

            var deltaMovement = _cameraTransform.position - _lastCameraPosition;
            _uvOffset += new Vector2(deltaMovement.x * parallaxMultiplier.x, deltaMovement.y * parallaxMultiplier.y);

            _rawImage.uvRect = new Rect(_uvOffset, _rawImage.uvRect.size);
            _lastCameraPosition = _cameraTransform.position;
        }
    }
}