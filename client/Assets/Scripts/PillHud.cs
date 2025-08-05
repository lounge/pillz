using TMPro;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class PillHud : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI usernameText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private Vector3 offset = new(0, 0.2f, 0); // height above character

        private Transform _target;
        private Camera _mainCamera;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        public void SetUsername(string username)
        {
            if (usernameText)
                usernameText.text = username;
        }

        public void SetHp(uint hp)
        {
            if (hpText)
                hpText.text = $"{hp} mg";
        }

        public void AttachTo(Transform target)
        {
            _target = target;
        }

        private void LateUpdate()
        {
            if (!_target) return;
            
            // Calculate a scale factor based on how zoomed out we are
            // You can tune the multiplier (e.g. 0.5f) to control how much it moves with zoom
            var zoomFactor = _mainCamera.orthographicSize * 0.2f;
            
            zoomFactor = Mathf.Clamp(zoomFactor, 0.2f, 10f);

            // Apply dynamic offset based on zoom level
            var scaledOffset = offset + new Vector3(0, zoomFactor, 0);

            // Convert world position with scaled offset to screen space
            var screenPosition = _mainCamera.WorldToScreenPoint(_target.position + scaledOffset);

            transform.position = screenPosition;
            transform.rotation = Quaternion.identity;
        }
    }
}