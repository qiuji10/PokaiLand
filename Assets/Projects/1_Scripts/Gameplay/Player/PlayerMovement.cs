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
        private float _lastReportedVelocityX;
        private int _layer;
        private Rigidbody2D _rb;
        private InputAction _moveAction;
        private PlayerControls _playerControls;

        private Vector2 MovementInput => _moveAction.ReadValue<Vector2>();

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _playerControls = new PlayerControls();
            _moveAction = _playerControls.Player.Move;
        }
        
        public override void OnNetworkSpawn()
        {
            // To make other player as jump-able ground
            _layer = LayerMask.NameToLayer(IsOwner ? "Player" : "Default");
            gameObject.layer = _layer != -1 ? _layer : 0;
            
            if (IsOwner)
            {
                EventBus.Register<ClientEnterDoorEvent>(OnInteractDoor);
                _playerControls.Enable();
                _playerControls.Player.Jump.performed += OnJump;
            }
        }
        
        public override void OnNetworkDespawn()
        {
            if (IsOwner)
            {
                EventBus.Unregister<ClientEnterDoorEvent>(OnInteractDoor);
                _playerControls.Player.Jump.performed -= OnJump;
                _playerControls.Disable();
            }
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

            return overlapCollider && overlapCollider.gameObject != gameObject;
        }

        private void OnInteractDoor(ClientEnterDoorEvent e)
        {
            if (e.ClientId != OwnerClientId) return;

            if (e.IsEnterDoor)
            {
                _playerControls.Player.Disable();
            }
            else
            {
                _playerControls.Player.Enable();
            }

            ServerSetPlayerToInvisibleLayerRpc(e.IsEnterDoor);
        }

        [Rpc(SendTo.Server, DeferLocal = true)]
        private void ServerSetPlayerToInvisibleLayerRpc(bool isInvisible)
        {
            ClientSetPlayerToInvisibleLayerRpc(isInvisible);
        }

        [Rpc(SendTo.ClientsAndHost, DeferLocal = true)]
        private void ClientSetPlayerToInvisibleLayerRpc(bool isInvisible)
        {
            gameObject.layer = isInvisible ? LayerMask.NameToLayer("Invisible") : _layer;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = IsGrounded() ? Color.green : Color.red;
            Gizmos.DrawWireCube((Vector2)transform.position + groundCheckOffset, groundCheckBoxSize);
        }
    }
}
