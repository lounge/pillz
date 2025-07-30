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

        [NonSerialized]
        private Rigidbody2D _rb;
        
        private PlayerInputActions _inputActions;
        private Vector2 _moveInput;
        private bool _isJumpPressed;
        private bool _isGrounded;
        private float _airborneXDirection = 0f;
        private float _lastMovementSendTimestamp;
        
        [NonSerialized]
        public WeaponController WeaponController;
        
        private PlayerInput _lastMovementInput;

        
        // [NonSerialized]
        // public MaskController MaskController;

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
                Log.Debug("MaskMovement: Not local player or not connected, skipping movement update.");
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

            _isJumpPressed = false;

            var playerInput = new PlayerInput(_rb.linearVelocity, !FocusHandler.HasFocus, _isGrounded);

            if (!playerInput.Equals(_lastMovementInput))
            {
                Debug.Log("Player Input Updated");
                GameManager.Connection.Reducers.UpdatePlayerInput(playerInput);
            }
            
            
            _lastMovementInput = playerInput;
            
            // Debug.Log("Player Input Updated: " + _rb.linearVelocity);
        }
    
    
        public void Spawn(Mask mask, PlayerController owner)
        {
            base.Spawn(mask.EntityId, owner);
            
            if (Owner && (!Owner.IsLocalPlayer || !GameManager.IsConnected()))
            {
                Log.Debug("MaskMovement: Not local player or not connected, skipping movement init.");
                return;
            }
            
            WeaponController = Instantiate(weaponPrefab, transform);
            WeaponController.Initialize(transform);
            
            _rb = GetComponent<Rigidbody2D>();
            _inputActions.Player.Jump.performed += _ => _isJumpPressed = true;
            _inputActions.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
        }

        // public void SetMask(MaskController entityController)
        // {
        //     MaskController = entityController;
        // }
    }
}