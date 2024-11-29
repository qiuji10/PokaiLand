using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

using PokaiLand.Events;

namespace PokaiLand.Player
{
    using Input;

    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float airMoveThreshold = 0.2f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.5f);
        [SerializeField] private Vector2 groundCheckBoxSize = new Vector2(1f,  0.1f);

        private bool _prevIsGrounded;
        private Rigidbody2D _rb;
        private InputAction _moveAction;
        private PlayerControls _playerControls;
        private float _lastReportedVelocityX;

        private Vector2 MovementInput => _moveAction.ReadValue<Vector2>();

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                // To make other player as jump-able ground 
                gameObject.layer = LayerMask.NameToLayer("Ground") != -1 ? LayerMask.NameToLayer("Ground") : 0;
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

        private void FixedUpdate()
        {
            if (!IsOwner) return;

            bool isGrounded = IsGrounded();
            Vector2 velocity = _rb.linearVelocity;

            if (!_prevIsGrounded && isGrounded)
            {
                velocity.y = 0f; // If remove this, then player will bounce when interact with RigidBody2D
                EventBus.Execute(new PlayerLandedEvent());
            }

            _prevIsGrounded = isGrounded;

            // Handle movement
            velocity.x = MovementInput.x * moveSpeed * (isGrounded ? 1 : airMoveThreshold);
            _rb.linearVelocity = velocity;

            // Flip sprite
            if (Mathf.Abs(MovementInput.x) > 0.1f)
            {
                Vector3 scale = transform.localScale;
                scale.x = MovementInput.x > 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                transform.localScale = scale;
            }

            // Report movement changes
            if (Mathf.Abs(velocity.x - _lastReportedVelocityX) > Mathf.Epsilon)
            {
                EventBus.Execute(new PlayerMovementEvent(velocity.x));
                _lastReportedVelocityX = velocity.x;
            }
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            if (!IsGrounded()) return;
            _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpForce);
            EventBus.Execute(new PlayerJumpEvent());
        }

        private bool IsGrounded()
        {
            var overlapCollider = Physics2D.OverlapBox(
                (Vector2)transform.position + groundCheckOffset,
                groundCheckBoxSize,
                0f,
                groundLayer
            );

            return overlapCollider && !overlapCollider.isTrigger && overlapCollider.gameObject != gameObject;
        }


        private void OnDrawGizmos()
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireCube((Vector2)transform.position + groundCheckOffset, groundCheckBoxSize);
        }
    }
}
