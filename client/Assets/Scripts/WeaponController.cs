using masks.client.Assets.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace masks.client.Scripts
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private Transform weapon;
        [SerializeField] private float weaponDistance = 1.5f;
        
        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float projectileSpeed = 20f;
        
        private Camera _mainCamera;
        private PlayerInputActions _inputActions;
        private Vector2 _lookInput;
        private Vector2 _weaponDirection;
        private Rigidbody2D _mask;

        private void OnEnable() => _inputActions.Enable();
        private void OnDisable() => _inputActions.Disable();
        
        private Transform _parentTransform;

        public void SetParent(Transform parent)
        {
            _parentTransform = parent;
        }

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            _inputActions.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Attack.performed += Shoot;
            _mainCamera = Camera.main;
        }

        private void Update()
        {
            if (_lookInput.sqrMagnitude > 0.01f)
            {
                var mouseWorldPosition = _mainCamera.ScreenToWorldPoint(_lookInput);
                _weaponDirection = (mouseWorldPosition - _parentTransform.position);
                
                var angle = Mathf.Atan2(_weaponDirection.y, _weaponDirection.x) * Mathf.Rad2Deg;
                weapon.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
                
                var weaponPosition = _parentTransform.position +  Quaternion.Euler(0, 0, angle) * new Vector3(weaponDistance, 0, 0);
                weapon.position = weaponPosition;
            }
        }

        private void Shoot(InputAction.CallbackContext ctx)
        {
            var projectile = Instantiate(projectilePrefab, weapon.position, Quaternion.identity);
            projectile.GetComponent<Rigidbody2D>().linearVelocity = _weaponDirection.normalized * projectileSpeed;
        }
    }
}