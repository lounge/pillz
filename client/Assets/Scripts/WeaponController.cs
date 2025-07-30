using masks.client.Assets.Input;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
using UnityEngine.InputSystem;

namespace masks.client.Scripts
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private Transform weapon;
        [SerializeField] private float weaponDistance = 1.5f;
        
        [Header("Projectile Settings")]
        [SerializeField] private ProjectileController projectilePrefab;
        [SerializeField] private float projectileSpeed = 20f;
        
        private Camera _mainCamera;
        private PlayerInputActions _inputActions;
        private Vector2 _lookInput;
        private Vector2 _weaponDirection;
        private Rigidbody2D _mask;

        private void OnEnable() => _inputActions.Enable();
        private void OnDisable() => _inputActions.Disable();
        
        private Transform _parentTransform;
        private Rigidbody2D _projectileRb;
        private PlayerController _owner;

        public void Initialize(Transform parent)
        {
            _parentTransform = parent;
        }

        private void Awake()
        {
            _inputActions = new PlayerInputActions();
            _inputActions.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Attack.performed += OnClick;
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
                
                // Log.Debug("WeaponDirection: " + _weaponDirection.normalized);
            }
            
            
        }

        private void OnClick(InputAction.CallbackContext ctx)
        {
       
            // Log.Debug("WeaponController: OnClick - Shooting projectile.");
            
            
            // GameManager.Entities.Add(projectile);
            GameManager.Connection.Reducers.ShootProjectile(new DbVector2(weapon.position.x, weapon.position.y));
            
        }
        
        public ProjectileController Shoot(Projectile projectile, PlayerController player)
        {
            Log.Debug($"WEAPON POSITION {weapon.position}");
            var projectileController = Instantiate(projectilePrefab, weapon.position, Quaternion.identity);

            // projectileController.transform.position = weapon.position;
            projectileController.Spawn(projectile, player, _weaponDirection.normalized * projectileSpeed);
            
            Log.Debug($"WeaponController: Shooting projectile from {weapon.position} with direction {_weaponDirection.normalized} and speed {projectileSpeed}.");
            
            
            // _projectileRb = projectile.GetComponent<Rigidbody2D>();
            // _projectileRb.linearVelocity = _weaponDirection.normalized * projectileSpeed;
            
            // Log.Debug($"WeaponController: Shooting projectile from {weapon.position} with direction {linearVelocity.linearVelocity} and speed {projectileSpeed}.");

            return projectileController;
        }
    }
}