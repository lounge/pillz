using pillz.client.Assets.Input;
using SpacetimeDB.Types;
using UnityEngine;
using UnityEngine.InputSystem;

namespace pillz.client.Scripts
{
    public class WeaponController : MonoBehaviour
    {
        [SerializeField] private Transform weapon;
        [SerializeField] private float weaponDistance = 1.5f;
        [SerializeField] private float projectileSpeed = 20f;

        [Header("Projectile Settings")] [SerializeField]
        private ProjectileController projectilePrefab;

        private Camera _mainCamera;
        private PlayerInputActions _inputActions;
        private Vector2 _lookInput;
        private Vector2 _weaponDirection;
        private Rigidbody2D _pill;
        private Transform _parentTransform;
        private Rigidbody2D _projectileRb;
        private PlayerController _owner;
        private Vector2 _aimDir;
        private float _fireStartTime;
        private WeaponType _type;

        public void Enable()
        {
            gameObject.SetActive(true);
            weapon.gameObject.SetActive(true);
        }

        public void Disable()
        {
            gameObject.SetActive(false);
            weapon.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            if (_owner?.IsLocalPlayer == true)
                _inputActions?.Enable();
        }

        private void OnDisable()
        {
            if (_owner?.IsLocalPlayer == true)
                _inputActions?.Disable();
        }

        public void Init(WeaponType type, Transform parent, PlayerController owner, DbVector2 aimDir)
        {
            _type = type;
            _owner = owner;
            _parentTransform = parent;
            _mainCamera = Camera.main;
            _aimDir = aimDir;

            if (_owner.IsLocalPlayer)
            {
                _inputActions = new PlayerInputActions();
                _inputActions.Player.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();

                _inputActions.Player.PrimaryAttack.started += OnClick;
                _inputActions.Player.PrimaryAttack.canceled += ctx => OnRelease(ctx, _type);

                _inputActions.Enable();
            }
        }

        public void SetAimDir(Vector2 aimDir)
        {
            _aimDir = aimDir;
        }

        private void Update()
        {
            Vector2 direction;

            if (_owner.IsLocalPlayer)
            {
                if (_lookInput.sqrMagnitude < 0.01f)
                    return;

                var mouseWorldPosition = _mainCamera.ScreenToWorldPoint(_lookInput);
                direction = (mouseWorldPosition - _parentTransform.position);

                GameInit.Connection.Reducers.Aim(_lookInput);
            }
            else
            {
                if (_aimDir.sqrMagnitude < 0.01f)
                    return;

                direction = _aimDir;
            }

            _weaponDirection = direction;

            var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            weapon.rotation = Quaternion.Euler(0, 0, angle - 90);

            var weaponPosition = _parentTransform.position +
                                 Quaternion.Euler(0, 0, angle) * new Vector3(weaponDistance, 0, 0);
            weapon.position = weaponPosition;
        }

 
        public ProjectileController Shoot(Projectile projectile, PlayerController player, Vector2 position, float speed)
        {
            var projectileController = Instantiate(projectilePrefab, weapon.position, Quaternion.identity);
            projectileController.Spawn(projectile, player, position, _weaponDirection.normalized * speed);

            return projectileController;
        }
        
        public void OnClick(InputAction.CallbackContext ctx)
        {
            _fireStartTime = Time.time;
        }

        private void OnRelease(InputAction.CallbackContext ctx, WeaponType type)
        {
            var heldDuration = Time.time - _fireStartTime;
            var durationClamp = Mathf.Clamp(heldDuration, 0.5f, 2f);
            var speed = projectileSpeed * durationClamp * 3;

            Debug.Log($"Mouse was held for {heldDuration} seconds. Speed: {speed:0.00}");

            GameInit.Connection.Reducers.ShootProjectile(new DbVector2(weapon.position.x, weapon.position.y), speed);
        }
    }
}