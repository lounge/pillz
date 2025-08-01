using System;
using masks.client.Assets.Input;
using SpacetimeDB;
using SpacetimeDB.Types;
using UnityEngine;

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
        
        [Header("Weapons")]
        public WeaponController weaponPrefab;
        
        [Header("Gui")]
        public HpDisplay hpDisplay;

        private uint _hp = 100;
        private Vector2 _moveInput;
        private bool _isJumpPressed;
        private bool _isGrounded;
        private float _airborneXDirection = 0f;
        private float _lastMovementSendTimestamp;
        private PlayerInputActions _inputActions;
        private PlayerInput _lastMovementInput;
        private HpDisplay _hpDisplay;
        
        [NonSerialized]
        private Rigidbody2D _rb;
        
        [NonSerialized]
        public WeaponController WeaponController;
        

        protected override void Awake()
        {
            _inputActions = new PlayerInputActions();
        }

        private void OnEnable() => _inputActions.Enable();
        private void OnDisable() => _inputActions.Disable();

        private void FixedUpdate()
        {
            if (!Owner.IsLocalPlayer || !GameManager.IsConnected())
            {
                // Log.Debug("MaskMovement: Not local player or not connected, skipping movement update.");
                return;
            }
            
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            if (_isJumpPressed && _isGrounded)
            {
                _rb.linearVelocityY = jumpForce;
            }

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
            
            _hpDisplay.transform.position = _rb.transform.position;

            var playerInput = new PlayerInput(_rb.linearVelocity, _rb.position,!FocusHandler.HasFocus);
            if (!playerInput.Equals(_lastMovementInput) && Time.time -_lastMovementSendTimestamp >= SendUpdatesFrequency)
            {
                Debug.Log("Player Input Updated");

                _lastMovementSendTimestamp = Time.time;
                GameManager.Connection.Reducers.UpdatePlayerInput(playerInput);
            }
            
            if (IsOutOfBounds())
            {
                // DEAD
                Log.Debug("MaskController: Out of bounds DEAD, deleting mask.");
                GameManager.Connection.Reducers.DeleteMask(null);
            }
            
            _lastMovementInput = playerInput;
            _isJumpPressed = false;
        }

        public void OnMaskUpdated(Mask newVal)
        {
            _hp = newVal.Hp;
            _hpDisplay.SetHp(_hp);
        }


        public void Spawn(Mask mask, PlayerController owner)
        {
            base.Spawn(mask.EntityId, owner);
            
            WeaponController = Instantiate(weaponPrefab, transform);
            WeaponController.Initialize(transform, owner);
            
            _hpDisplay = Instantiate(hpDisplay, transform.position, Quaternion.identity);
            _hpDisplay.transform.SetParent(null);
            _hpDisplay.AttachTo(transform);
            _hpDisplay.SetHp(mask.Hp);    
            
            if (Owner && (!Owner.IsLocalPlayer || !GameManager.IsConnected()))
            {
                Log.Debug("MaskMovement: Not local player or not connected, skipping movement init.");
                return;
            }
            
            _rb = GetComponent<Rigidbody2D>();
            
            _inputActions.Player.Jump.performed += _ => _isJumpPressed = true;
            _inputActions.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
        }

        public void ApplyDamage(uint damage)
        {
            GameManager.Connection.Reducers.ApplyDamage(Owner.PlayerId, damage);
            
            if (_hp <= 0)
            {
                Log.Debug("MaskController: HP is 0 or below, deleting mask.");
                GameManager.Connection.Reducers.DeleteMask(Owner.PlayerId);
                return;
            }
            
            Log.Debug($"MaskController: Applied {damage} damage, remaining HP: {_hp}");
        }

        public override void OnDelete(EventContext context)
        {
            base.OnDelete(context);
            
            if (_hpDisplay)
            {
                Destroy(_hpDisplay.gameObject);
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
    }
}