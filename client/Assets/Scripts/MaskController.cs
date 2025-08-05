using System;
using masks.client.Assets.Input;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
using UnityEngine.Serialization;

namespace masks.client.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class MaskController : EntityController
    {
        [Header("Movement Settings")] public float moveSpeed = 10f;
        public float jumpForce = 10f;
        public LayerMask groundLayer;
        public Transform groundCheck;
        public float groundCheckRadius = 0.2f;

        [Header("Air Control")] [Range(0.8f, 1f)]
        public float airDragFactor = 0.95f;

        [Range(0f, 1f)] public float smoothing = 0.15f;

        [Header("Weapons")] public WeaponController weaponPrefab;

        [Header("Jetpack")] public JetpackController jetpack;

        [Header("Gui")] public MaskHud maskHud;
        public DmgDisplay dmgDisplay;
        public FragDisplay fragDisplay;


        private uint _hp = 100;
        private Vector2 _moveInput;
        private bool _isJumpHeld;
        private bool _isJetpackEnabled;
        private bool _isGrounded;
        private float _airborneXDirection = 0f;
        private float _lastMovementSendTimestamp;
        private PlayerInputActions _inputActions;
        private PlayerInput _lastMovementInput;
        private MaskHud _maskHud;
        private DmgDisplay _dmgDisplay;
        private FragDisplay _fragDisplay;
        private GameObject _gameCanvas;

        [NonSerialized] private Camera _mainCamera;
        [NonSerialized] private Rigidbody2D _rb;
        [NonSerialized] public WeaponController WeaponController;
        [NonSerialized] public bool InPortal;
        [NonSerialized] public float PortalCoolDown;

        protected override void Awake()
        {
            jetpack?.gameObject.SetActive(false);
            _gameCanvas = GameObject.Find("GameCanvas");
            _mainCamera = Camera.main;
            _inputActions = new PlayerInputActions();
        }

        private void OnEnable() => _inputActions.Enable();
        private void OnDisable() => _inputActions.Disable();

        public void Spawn(Mask mask, PlayerController owner)
        {
            base.Spawn(mask.EntityId, owner);

            // Set position from server
            transform.position = new Vector3(mask.Position.X, mask.Position.Y, 0);

            WeaponController = Instantiate(weaponPrefab, transform);
            WeaponController.Initialize(transform, owner, mask.AimDir);

            _maskHud = Instantiate(maskHud, _gameCanvas.transform);
            _maskHud.AttachTo(transform);
            _maskHud.SetHp(mask.Hp);
            _maskHud.SetUsername(owner.Username);

            if (Owner && (!Owner.IsLocalPlayer || !GameManager.IsConnected()))
            {
                Log.Debug("MaskMovement: Not local player or not connected, skipping movement init.");
                return;
            }

            _rb = GetComponent<Rigidbody2D>();
            _mainCamera.GetComponent<CameraFollow>()?.SetTarget(transform);

            _inputActions.Player.Jump.started += _ => _isJumpHeld = true;
            _inputActions.Player.Jump.canceled += _ => _isJumpHeld = false;
            _inputActions.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
            _inputActions.Player.Jetpack.performed += _ => _isJetpackEnabled = !_isJetpackEnabled;

            _dmgDisplay = Instantiate(dmgDisplay, Owner.transform);
            dmgDisplay.SetDmg(mask.Dmg);

            _fragDisplay = Instantiate(fragDisplay, Owner.transform);
            fragDisplay.SetFrags(mask.Frags);
        }

        private void FixedUpdate()
        {
            if (!Owner.IsLocalPlayer || !GameManager.IsConnected())
            {
                // Log.Debug("MaskMovement: Not local player or not connected, skipping movement update.");
                return;
            }

            CheckPortalState();

            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (_isJetpackEnabled)
            {
                JetpackMovement();
            }
            else
            {
                NormalMovement();
            }

            var playerInput = new PlayerInput(_rb.linearVelocity, _rb.position, !FocusHandler.HasFocus);
            if (!playerInput.Equals(_lastMovementInput) &&
                Time.time - _lastMovementSendTimestamp >= SendUpdatesFrequency)
            {
                _lastMovementSendTimestamp = Time.time;
                GameManager.Connection.Reducers.UpdatePlayerInput(playerInput);
            }

            _lastMovementInput = playerInput;
        }

        private void NormalMovement()
        {
            jetpack?.Disable();

            if (_isJumpHeld && _isGrounded)
            {
                Log.Debug("MaskMovement: Jump pressed, applying jump force.");
                _rb.linearVelocityY = jumpForce;
            }

            CalculateMovement();
        }

        private void JetpackMovement()
        {
            jetpack?.Enable();
            Log.Debug("JetpackMovement: Jetpack is enabled, applying jetpack force.");

            if (_isJumpHeld)
            {
                Log.Debug("MaskMovement: Jump pressed, applying jump force.");
                jetpack?.ThrottleOn(1);
                _rb.linearVelocityY = Mathf.Lerp(_rb.linearVelocityY, jumpForce, 0.2f);
            }
            else
            {
                jetpack?.ThrottleOff();
            }

            CalculateMovement();
        }

        private void CalculateMovement()
        {
            var inputX = _moveInput.x;
            float targetX;

            if (_isGrounded)
            {
                targetX = inputX * moveSpeed;
                _rb.linearVelocityX = Mathf.Lerp(_rb.linearVelocityX, targetX, smoothing);
                _airborneXDirection = inputX;
            }
            else
            {
                if (Mathf.Abs(inputX) > 0.01f)
                {
                    _airborneXDirection = inputX;
                }

                targetX = _airborneXDirection * moveSpeed;
                _rb.linearVelocityX *= airDragFactor;
                _rb.linearVelocityX = Mathf.Lerp(_rb.linearVelocityX, targetX, smoothing);
            }

            _maskHud.transform.position = _rb.transform.position;
        }

        public void OnMaskUpdated(Mask newVal)
        {
            _hp = newVal.Hp;
            _maskHud?.SetHp(_hp);
            _dmgDisplay?.SetDmg(newVal.Dmg);
            _fragDisplay?.SetFrags(newVal.Frags);
            WeaponController?.SetAimDir(newVal.AimDir);

            if (_hp <= 0)
            {
                Log.Debug("MaskController: HP is 0 or below, deleting mask.");
                Kill();
            }
        }


        public void ApplyDamage(uint damage)
        {
            GameManager.Connection.Reducers.ApplyDamage(Owner.PlayerId, damage);

            Log.Debug($"MaskController: Applied {damage} damage, remaining HP: {_hp}");
        }

        public override void OnDelete(EventContext context)
        {
            base.OnDelete(context);

            if (_maskHud)
            {
                Destroy(_maskHud.gameObject);
            }

            if (WeaponController)
            {
                Destroy(WeaponController.gameObject);
            }

            if (Owner)
            {
                Destroy(Owner.gameObject);
            }

            _inputActions?.Disable();
            _inputActions = null;
        }

        private void CheckPortalState()
        {
            if (PortalCoolDown > 0f)
            {
                PortalCoolDown -= Time.deltaTime;
            }
            else
            {
                InPortal = false;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("DeathZone"))
            {
                Log.Debug("MaskController: Collided with death zone DEAD, deleting mask.");
                Kill();
            }
        }

        private void Kill()
        {
            if (Owner.IsLocalPlayer)
            {
                Log.Debug("MaskController: Local player mask destroyed, showing death screen.");
                DeathScreenManager.Instance.Show(Owner);
            }

            GameManager.Connection.Reducers.DeleteMask(Owner.PlayerId);
        }
    }
}