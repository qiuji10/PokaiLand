using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

using PokaiLand.Events;


namespace PokaiLand.Player
{
    using static InputKeys;
    using Input;
    
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float airMoveThreshold = 0.2f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.5f); // Offset for ground check
        [SerializeField] private float groundCheckRadius = 0.1f; 
        
        private bool _prevIsGrounded;
        private Rigidbody2D _rb;
        private Collider2D _collider;
        private InputAction _moveAction;
        private PlayerControls _playerControls;

        private Vector2 MovementInput => _moveAction.ReadValue<Vector2>();

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                gameObject.layer = LayerMask.NameToLayer("Default");
            }
        }

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _playerControls = new PlayerControls();
            _moveAction = _playerControls.Player.Move;
        }

        private void OnEnable()
        {
            _playerControls.Enable();
            _playerControls.Player.Jump.performed += OnJump;
        }

        private void OnDisable()
        {
            _playerControls.Disable();
            _playerControls.Player.Jump.performed -= OnJump;
        }

        private void Update()
        {
            if (!IsOwner) return; 
            
            bool isGrounded = IsGrounded();

            if (!_prevIsGrounded && isGrounded)
                EventBus.Execute(new PlayerLandedEvent());
            
            _prevIsGrounded = isGrounded;

            if (MovementInput != Vector2.zero)
            {
                if (isGrounded)
                    _rb.linearVelocityX = MovementInput.x * moveSpeed;
                else
                    _rb.linearVelocityX = MovementInput.x * moveSpeed * airMoveThreshold;
                
                transform.eulerAngles = new Vector3(0, _rb.linearVelocityX > 0 ? 180 : 0, 0);
                EventBus.Execute(new PlayerMovementEvent(_rb.linearVelocityX));
            }
            else
            {
                //if (_rb.linearVelocityX != 0)
                    
                EventBus.Execute(new PlayerMovementEvent(0));
                _rb.linearVelocityX = 0;
            }
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            if (!IsGrounded()) return;
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            EventBus.Execute(new PlayerJumpEvent());
        }

        private bool IsGrounded()
        {
            var overlapCollider = Physics2D.OverlapCircle((Vector2)transform.position + groundCheckOffset, groundCheckRadius, groundLayer);

            if (!overlapCollider)
                return false;
            
            return overlapCollider.gameObject != gameObject;
        }
        
        private void OnDrawGizmos()
        {
            // Draw IsGround()
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireSphere((Vector2)transform.position + groundCheckOffset, groundCheckRadius);
        }
    }
}

