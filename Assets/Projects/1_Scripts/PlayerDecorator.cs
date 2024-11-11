using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Player
{
    public class PlayerDecorator : NetworkBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        public NetworkVariable<Color> playerColor = new();

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
                spriteRenderer.color = playerColor.Value;
            
            playerColor.OnValueChanged += OnPlayerColorChanged;
        }

        public override void OnNetworkDespawn()
        {
            playerColor.OnValueChanged -= OnPlayerColorChanged;
        }

        private void OnPlayerColorChanged(Color previousvalue, Color newvalue)
        {
            spriteRenderer.color = newvalue;
        }
    }
}