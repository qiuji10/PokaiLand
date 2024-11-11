using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PokaiLand.Player
{
    using static InputKeys;
    
    public class PlayerMovement : NetworkBehaviour
    {
        [Header("Configuration")]
        [SerializeField] private float moveSpeed = 10f;
        [SerializeField] private float airMoveThreshold = 0.2f;
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Vector2 groundCheckOffset = new Vector2(0f, -0.5f); // Offset for ground check
        [SerializeField] private float groundCheckRadius = 0.1f; 
        
        [Header("References")]
        [SerializeField] private InputActionAsset inputAsset;
        
        private Rigidbody2D _rb;
        private Collider2D _collider;
        private InputAction _moveAction;

        private Vector2 Movement => _moveAction.ReadValue<Vector2>();

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
            _moveAction = inputAsset.FindActionMap(PlayerMap).FindAction(MoveAction);
        }

        private void OnEnable()
        {
            inputAsset.FindActionMap(PlayerMap).Enable();
            inputAsset.FindActionMap(PlayerMap).FindAction(JumpAction).performed += OnJump;
        }

        private void OnDisable()
        {
            inputAsset.FindActionMap(PlayerMap).FindAction(JumpAction).performed -= OnJump;
            inputAsset.FindActionMap(PlayerMap).Disable();
        }

        private void FixedUpdate()
        {
            if (Movement != Vector2.zero)
            {
                if (IsGrounded())
                    _rb.linearVelocityX = Movement.x * moveSpeed;
                else
                    _rb.linearVelocityX = Movement.x * moveSpeed * airMoveThreshold;
            }
            else
            {
                _rb.linearVelocityX = 0;
            }
        }

        private void OnJump(InputAction.CallbackContext ctx)
        {
            if (IsGrounded())
                _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
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

