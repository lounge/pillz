using UnityEngine;
using masks.client.Assets.Input;
namespace masks.client.Scripts
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement Settings")] 
        public float moveSpeed = 40f;
        public float jumpForce = 10f;
        public LayerMask groundLayer;
        public Transform groundCheck;
        public float groundCheckRadius = 0.2f;
        
        [Header("Air Control")] 
        [Range(0.8f, 1f)] public float airDragFactor = 0.95f;
        [Range(0f, 1f)] public float smoothing = 0.15f;

        private Rigidbody2D _rb;
        private PlayerInputActions _inputActions;
        private Vector2 _moveInput;
        private bool _isJumpPressed;
        private bool _isGrounded;
        private float _airborneXDirection = 0f;


        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _inputActions = new PlayerInputActions();
            _inputActions.Player.Jump.performed += _ => _isJumpPressed = true;
            _inputActions.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
            _inputActions.Player.Move.canceled += ctx => _moveInput = Vector2.zero;
        }

        private void OnEnable() => _inputActions.Enable();
        private void OnDisable() => _inputActions.Disable();

        private void FixedUpdate()
        {
            // Check ground
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            // Jump
            if (_isJumpPressed && _isGrounded)
            {
                _rb.linearVelocityY = jumpForce;
            }
            
            float inputX = _moveInput.x;
            float targetX;

            if (_isGrounded)
            {
                // Ground: smooth toward input
                targetX = inputX * moveSpeed;
                _rb.linearVelocityX = Mathf.Lerp(_rb.linearVelocityX, targetX, smoothing);

                _airborneXDirection = inputX; // Reset
            }
            else
            {
                // In air: allow directional input
                if (Mathf.Abs(inputX) > 0.01f)
                {
                    _airborneXDirection = inputX;
                }

                // Desired velocity including directional push
                targetX = _airborneXDirection * moveSpeed;

                // Apply drag first
                _rb.linearVelocityX *= airDragFactor;

                // Smooth toward the adjusted velocity
                _rb.linearVelocityX = Mathf.Lerp(_rb.linearVelocityX, targetX, smoothing);
            }
            
            _isJumpPressed = false; // Reset flag each frame
        }
    }
}