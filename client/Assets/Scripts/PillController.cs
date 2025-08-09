using System;
using JetBrains.Annotations;
using pillz.client.Assets.Input;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;
using PlayerInput = SpacetimeDB.Types.PlayerInput;

namespace pillz.client.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PillController : EntityController
    {
        [Header("Movement Settings")] public float moveSpeed = 10f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.2f;

        [Header("Air Control")] [Range(0.8f, 1f)] [SerializeField]
        private float airDragFactor = 0.95f;

        [Range(0f, 1f)] public float smoothing = 0.15f;

        [Header("Weapons")] [SerializeField] private WeaponController primaryWeaponPrefab;
        [SerializeField] private WeaponController secondaryWeaponPrefab;

        [Header("Jetpack")] [SerializeField] private JetpackController jetpack;

        [Header("Gui")] [SerializeField] private PillHud pillHud;

        private uint _hp = 100;
        private Vector2 _moveInput;
        private bool _isJumpHeld;
        private bool _jetpackClick;
        private bool _isGrounded;
        private float _airborneXDirection;
        private float _lastMovementSendTimestamp;
        private PlayerInputActions _inputActions;
        private PlayerInput _lastMovementInput;
        private PillHud _pillHud;
        private GameObject _pillCanvas;
        private GameObject _gameHud;
        private WeaponController _primaryWeapon;
        private WeaponController _secondaryWeapon;

        [NonSerialized] private Rigidbody2D _rb;
        [NonSerialized] private DmgDisplay _dmgDisplay;
        [NonSerialized] private FragDisplay _fragDisplay;
        [NonSerialized] private Camera _mainCamera;
        [NonSerialized] public bool InPortal;
        [NonSerialized] public float PortalCoolDown;
        private WeaponType _selectedWeapon;

        protected override void Awake()
        {
            _mainCamera = Camera.main;
            _inputActions = new PlayerInputActions();
            _pillCanvas = GameObject.Find("Pill HUD");
        }

        private void OnEnable() => _inputActions.Enable();
        private void OnDisable() => _inputActions.Disable();

        public void Spawn(Pill pill, PlayerController owner)
        {
            base.Spawn(pill.EntityId, owner);

            // Set position from server correction for client placement
            transform.position = new Vector3(pill.Position.X + 0.5f, pill.Position.Y + 2f, 0);

            _primaryWeapon = Instantiate(primaryWeaponPrefab, transform);
            _secondaryWeapon = Instantiate(secondaryWeaponPrefab, transform);
            _primaryWeapon.Init(WeaponType.Primary, transform, owner, pill.AimDir);
            _secondaryWeapon.Init(WeaponType.Secondary, transform, owner, pill.AimDir);
            _secondaryWeapon.Disable();

            _pillHud = Instantiate(pillHud, _pillCanvas.transform);
            _pillHud.AttachTo(transform);
            _pillHud.SetHp(pill.Hp);
            _pillHud.SetUsername(owner.Username);

            jetpack.Init(this, _pillHud);

            _rb = GetComponent<Rigidbody2D>();

            if (Owner && (!Owner.IsLocalPlayer || !GameHandler.IsConnected()))
            {
                Log.Debug("PillMovement: Not local player or not connected, skipping movement init.");
                return;
            }

            _mainCamera.GetComponent<CameraFollow>()?.SetTarget(transform);

            _inputActions.Player.Jump.started += _ => _isJumpHeld = true;
            _inputActions.Player.Jump.canceled += _ => _isJumpHeld = false;
            _inputActions.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Move.canceled += _ => _moveInput = Vector2.zero;
            _inputActions.Player.Jetpack.performed += _ => _jetpackClick = !_jetpackClick;
            _inputActions.Player.PrimaryWeapon.performed += _ =>
            {
                Log.Debug("PillMovement: Primary weapon selected.");
                _selectedWeapon = WeaponType.Primary;
            };
            _inputActions.Player.SecondaryWeapon.performed += _ =>
            {
                Log.Debug("PillMovement: Secondary weapon selected.");
                _selectedWeapon = WeaponType.Secondary;
            };
            
            var gameHud = GameObject.Find("Game HUD");
            _gameHud = Instantiate(Owner.GetHud(), gameHud.transform);

            _dmgDisplay = _gameHud.GetComponentInChildren<DmgDisplay>();
            _fragDisplay = _gameHud.GetComponentInChildren<FragDisplay>();

            _dmgDisplay.SetDmg(pill.Dmg);
            _fragDisplay.SetFrags(pill.Frags);
        }


        private void FixedUpdate()
        {
            if (!GameHandler.IsConnected() || !Owner.IsLocalPlayer)
            {
                return;
            }
            
            CheckPortalState();

            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
            if (_jetpackClick)
            {
                JetpackMovement();
            }
            else
            {
                NormalMovement();
            }
            
            var playerInput = new PlayerInput(_rb.linearVelocity, _rb.position, !FocusHandler.HasFocus, _selectedWeapon);
            if (Time.time - _lastMovementSendTimestamp >= SendUpdatesFrequency && !playerInput.Equals(_lastMovementInput))
            {
                _lastMovementSendTimestamp = Time.time;
                GameHandler.Connection.Reducers.UpdatePlayer(playerInput);
            }

            _lastMovementInput = playerInput;
        }

        private void NormalMovement()
        {
            jetpack?.Disable();

            if (_isJumpHeld && _isGrounded)
            {
                Log.Debug("PillMovement: Jump pressed, applying jump force.");
                _rb.linearVelocityY = jumpForce;
            }

            CalculateMovement();
        }

        private void JetpackMovement()
        {
            jetpack?.Enable();
            if (_isJumpHeld)
            {
                Log.Debug("PillMovement: Jump pressed, applying jump force.");
                jetpack?.ThrottleOn();
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
            if (IsOutOfBounds() == OutOfBound.Left)
            {
                transform.position = new Vector3(TerrainHandler.Instance.MaxX - 1f, transform.position.y, 0);
            }
            else if (IsOutOfBounds() == OutOfBound.Right)
            {
                transform.position = new Vector3(TerrainHandler.Instance.MinX + 1f, transform.position.y, 0);
            }

            var inputX = _moveInput.x;
            if (_isGrounded)
            {
                var targetX = inputX * moveSpeed;
                _rb.linearVelocityX = Mathf.Lerp(_rb.linearVelocityX, targetX, smoothing);
                _airborneXDirection = inputX;
            }
            else
            {
                if (Mathf.Abs(inputX) > 0.01f)
                {
                    _airborneXDirection = inputX;
                }

                var targetX = _airborneXDirection * moveSpeed;
                // _rb.linearVelocityX *= airDragFactor;
                _rb.linearVelocityX = Mathf.Lerp(_rb.linearVelocityX, targetX, smoothing);
            }

            _pillHud.transform.position = _rb.transform.position;
        }

        public void OnPillUpdated(Pill newVal)
        {
            _hp = newVal.Hp;
            _pillHud?.SetHp(_hp);;
            _dmgDisplay?.SetDmg(newVal.Dmg);
            _fragDisplay?.SetFrags(newVal.Frags);

            jetpack?.OnJetpackUpdated(newVal.Jetpack.Enabled, newVal.Jetpack.Throttling, newVal.Jetpack.Fuel);
           
            _primaryWeapon?.SetAimDir(newVal.AimDir);
            _secondaryWeapon?.SetAimDir(newVal.AimDir);

            ApplyForce(newVal.Force);
            SetWeapon(newVal);

            if (_hp <= 0)
            {
                Log.Debug("PillController: HP is 0 or below, deleting pill.");
                Kill();
            }
        }

        public EntityController Shoot(Projectile insertedValue, PlayerController player, Vector2 spawnPos,
            float insertedValueSpeed)
        {
            return _selectedWeapon switch
            {
                WeaponType.Primary => _primaryWeapon.Shoot(insertedValue, player, spawnPos, insertedValueSpeed),
                WeaponType.Secondary => _secondaryWeapon.Shoot(insertedValue, player, spawnPos, insertedValueSpeed),
                _ => throw new ArgumentOutOfRangeException(nameof(_selectedWeapon), _selectedWeapon, null)
            };
        }
        
        public void OnJetpackDepleted()
        {
            _jetpackClick = false;
        }

        public override void OnDelete(EventContext context)
        {
            base.OnDelete(context);

            if (_pillHud)
            {
                Destroy(_pillHud.gameObject);
            }

            if (_primaryWeapon)
            {
                Destroy(_primaryWeapon.gameObject);
            }

            if (_secondaryWeapon)
            {
                Destroy(_secondaryWeapon.gameObject);
            }

            if (Owner)
            {
                Destroy(Owner.gameObject);
            }

            if (_gameHud)
            {
                Destroy(_gameHud.gameObject);
            }

            _inputActions?.Disable();
            _inputActions = null;
        }

        private void ApplyForce([CanBeNull] DbVector2 force)
        {
            if (force is not null)
            {
                Log.Debug($"PillController: Applying force {force.X}, {force.Y} to pill.");
                _rb.AddForce(new Vector2(force.X, force.Y), ForceMode2D.Impulse);
                GameHandler.Connection.Reducers.ForceApplied(Owner.PlayerId);
            }
        }

        private void SetWeapon(Pill newVal)
        {
            switch (newVal.SelectedWeapon)
            {
                case WeaponType.Primary:
                    _secondaryWeapon?.Disable();
                    _primaryWeapon?.Enable();
                    _primaryWeapon?.SetAimDir(newVal.AimDir);
                    break;
                case WeaponType.Secondary:          
                    _primaryWeapon?.Disable();
                    _secondaryWeapon?.Enable();
                    _secondaryWeapon?.SetAimDir(newVal.AimDir);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
            if (collision.gameObject.CompareTag(Tags.DeathZone))
            {
                Log.Debug("PillController: Collided with death zone DEAD, deleting pill.");
                Kill();
            }
        }

        private void Kill()
        {
            if (Owner.IsLocalPlayer)
            {
                Log.Debug("PillController: Local player pill destroyed, showing death screen.");
                DeathScreenHandler.Instance.Show(Owner);
            }

            GameHandler.Connection.Reducers.DeletePill(Owner.PlayerId);
        }

       
    }
}