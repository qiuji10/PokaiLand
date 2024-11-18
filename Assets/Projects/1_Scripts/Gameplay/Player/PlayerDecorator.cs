using System;
using Unity.Netcode;
using UnityEngine;

using PokaiLand.Events;
using Unity.Collections;

namespace PokaiLand.Player
{
    public class PlayerDecorator : NetworkBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator anim;
        
        public NetworkVariable<Color> playerColor = new();
        public NetworkVariable<FixedString32Bytes> playerName = new();

        private bool _isGrounded;
        private readonly int ANIM_Player_Walk = Animator.StringToHash("ANIM_Player_Walk");
        private readonly int ANIM_Player_Idle = Animator.StringToHash("ANIM_Player_Idle");
        
        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                gameObject.name = playerName.Value.Value;
                spriteRenderer.color = playerColor.Value;
            }

            playerName.OnValueChanged += OnPlayerNameChanged;
            playerColor.OnValueChanged += OnPlayerColorChanged;
            EventBus.Register<PlayerMovementEvent>(OnPlayerMovement);
            EventBus.Register<PlayerJumpEvent>(OnPlayerJump);
            EventBus.Register<PlayerLandedEvent>(OnPlayerLanded);
        }

        public override void OnNetworkDespawn()
        {
            playerName.OnValueChanged += OnPlayerNameChanged;
            playerColor.OnValueChanged -= OnPlayerColorChanged;
            EventBus.Deregister<PlayerMovementEvent>(OnPlayerMovement);
            EventBus.Deregister<PlayerJumpEvent>(OnPlayerJump);
            EventBus.Deregister<PlayerLandedEvent>(OnPlayerLanded);
        }

        private void OnPlayerMovement(PlayerMovementEvent e)
        {
            if (!IsOwner) return; 
            
            if (!_isGrounded)
            {
                anim.Play(ANIM_Player_Idle);
                return;
            }
            
            if (e.horizontalVelocity != 0)
                anim.Play(ANIM_Player_Walk);
            else
                anim.Play(ANIM_Player_Idle);
        }
        
        private void OnPlayerJump(PlayerJumpEvent obj)
        {
            _isGrounded = false;
        }

        private void OnPlayerLanded(PlayerLandedEvent obj)
        {
            _isGrounded = true;
        }

        private void OnPlayerNameChanged(FixedString32Bytes previousValue, FixedString32Bytes newValue)
        {
            gameObject.name = newValue.Value;
        }
        
        private void OnPlayerColorChanged(Color previousValue, Color newValue)
        {
            spriteRenderer.color = newValue;
        }
    }
}