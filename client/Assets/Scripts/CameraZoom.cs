using masks.client.Assets.Input;
using UnityEngine;

namespace masks.client.Scripts
{
    public class CameraZoom : MonoBehaviour
    {
        [SerializeField] private float zoomStepSize = 5f;
        [SerializeField] private float zoomLerpSpeed = 10f; // Higher = faster transition
        [SerializeField] private float minZoom = 8f;
        [SerializeField] private float maxZoom = 100f;

        private PlayerInputActions _inputActions;
        private Camera _camera;

        private float _targetZoom;

        private void Awake()
        {
            _camera = Camera.main;
            _targetZoom = _camera!.orthographicSize;

            _inputActions = new PlayerInputActions();
            _inputActions.Player.Zoom.performed += OnZoom;
        }

        private void OnEnable() => _inputActions.Enable();
        private void OnDisable() => _inputActions.Disable();

        private void OnZoom(UnityEngine.InputSystem.InputAction.CallbackContext context)
        {
            var scroll = context.ReadValue<Vector2>().y;

            if (Mathf.Abs(scroll) > 0.01f)
            {
                var step = -Mathf.Sign(scroll) * zoomStepSize;
                _targetZoom = Mathf.Clamp(_targetZoom + step, minZoom, maxZoom);
            }
        }

        private void Update()
        {
            _camera.orthographicSize = Mathf.Lerp(
                _camera.orthographicSize,
                _targetZoom,
                Time.deltaTime * zoomLerpSpeed
            );
        }
    }
}