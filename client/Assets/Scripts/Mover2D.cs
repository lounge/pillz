using pillz.client.Scripts.ScriptableObjects.Pill;
using UnityEngine;

namespace pillz.client.Scripts
{
    public class Mover2D : MonoBehaviour
    {
        [SerializeField] private MovementConfig config;
        [SerializeField] private Transform groundCheck;

        private Rigidbody2D _rb;
        private bool _isGrounded;
        private float _airDirX;
        private bool _wasThrottling;

        public void Init(MovementConfig cfg, Rigidbody2D rb)
        {
            config = cfg;
            _rb = rb;
        }

        public void Tick(PillIntent intent, bool jetpackActive, bool jetpackThrottling)
        {
            _isGrounded = Physics2D.OverlapCircle(groundCheck.position, config.groundCheckRadius, config.groundLayer);

            if (!jetpackThrottling && _wasThrottling)
            {
                _airDirX = 0f; 
                if (!_isGrounded && Mathf.Abs(intent.Move.x) < 0.01f)
                {
                    _rb.linearVelocityX = 0f;
                }
            }

            _wasThrottling = jetpackThrottling;

            if (!jetpackThrottling && intent.JumpHeld && _isGrounded)
            {
                _rb.linearVelocityY = config.jumpForce;
            }

            if (jetpackThrottling)
            {
                JetpackHorizontal(intent.Move.x);
            }
            else
            {
                GroundAirHorizontal(intent.Move.x);
            }
        }

        public void JetpackLift(float verticalForce)
        {
            _rb.linearVelocityY = Mathf.Lerp(_rb.linearVelocityY, verticalForce, 0.2f);
        }

        private void GroundAirHorizontal(float inputX)
        {
            if (_isGrounded)
            {
                var targetX = inputX * config.moveSpeed;
                _rb.linearVelocityX = Mathf.Lerp(_rb.linearVelocityX, targetX, config.smoothing);
                _airDirX = inputX; 
            }
            else
            {
                if (Mathf.Abs(inputX) > 0.01f)
                {
                    _airDirX = inputX;
                }
                
                var targetX = _airDirX * config.moveSpeed;
                _rb.linearVelocityX = Mathf.Lerp(_rb.linearVelocityX, targetX, config.smoothing);
            }
        }

        private void JetpackHorizontal(float inputX)
        {
            if (Mathf.Abs(inputX) < 0.01f)
            {
                _rb.linearVelocityX = Mathf.MoveTowards(_rb.linearVelocityX, 0f, config.jetpackBrake * Time.fixedDeltaTime);
                return;
            }

            var target = inputX * config.jetpackStrafe;
            _rb.linearVelocityX = Mathf.MoveTowards(_rb.linearVelocityX, target, config.jetpackAccelerate * Time.fixedDeltaTime);
        }
    }
}