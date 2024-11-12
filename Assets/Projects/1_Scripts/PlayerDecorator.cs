using System;
using PokaiLand.Events;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Player
{
    public class PlayerDecorator : NetworkBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Animator anim;
        
        public NetworkVariable<Color> playerColor = new();

        private bool _isGrounded;
        private int ACLIP_Player_Walk = Animator.StringToHash("ACLIP_Player_Walk");
        private int ACLIP_Player_Idle = Animator.StringToHash("ACLIP_Player_Idle");
        
        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
                spriteRenderer.color = playerColor.Value;
            
            playerColor.OnValueChanged += OnPlayerColorChanged;
            EventBus.Register<PlayerMovementEvent>(OnPlayerMovement);
            EventBus.Register<PlayerJumpEvent>(OnPlayerJump);
            EventBus.Register<PlayerLandedEvent>(OnPlayerLanded);
        }
        
        public override void OnNetworkDespawn()
        {
            playerColor.OnValueChanged -= OnPlayerColorChanged;
            EventBus.Deregister<PlayerMovementEvent>(OnPlayerMovement);
            EventBus.Deregister<PlayerJumpEvent>(OnPlayerJump);
            EventBus.Deregister<PlayerLandedEvent>(OnPlayerLanded);
        }

        private void OnPlayerMovement(PlayerMovementEvent e)
        {
            if (!_isGrounded)
            {
                anim.Play(ACLIP_Player_Idle);
                return;
            }
            
            if (e.horizontalVelocity != 0)
                anim.Play(ACLIP_Player_Walk);
            else
                anim.Play(ACLIP_Player_Idle);
        }
        
        private void OnPlayerJump(PlayerJumpEvent obj)
        {
            _isGrounded = false;
        }

        private void OnPlayerLanded(PlayerLandedEvent obj)
        {
            _isGrounded = true;
        }

        private void OnPlayerColorChanged(Color previousvalue, Color newvalue)
        {
            spriteRenderer.color = newvalue;
        }
    }
}