using PokaiLand.Enum;
using PokaiLand.Input;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PokaiLand.Item
{
    public class Key : NetworkBehaviour, IPickable
    {
        [SerializeField] private bool canDrop;

        public bool IsPickedUp => _isPickedUp.Value;
        public bool CanDrop => canDrop;
        public ulong HolderClientId => _holderClientId.Value;
        public EInteractable Type => EInteractable.Key;
        public InputAction InputAction => new PlayerControls().Player.Interact;

        private readonly NetworkVariable<bool> _isPickedUp = new();
        private readonly NetworkVariable<ulong> _holderClientId = new();
        
        public void Interact(InteractInfo info)
        {
            if (!IsPickedUp)
                OnPick(info.ClientId);
            else
                OnDrop();
        }
        
        public void OnPick(ulong pickerClientId)
        {
            OnPickServerRpc(pickerClientId);
        }

        [Rpc(SendTo.Server)]
        private void OnPickServerRpc(ulong pickerClientId)
        {
            Debug.Log($"{pickerClientId} picked");
            NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(pickerClientId, out var clientNetworkObject);
            _holderClientId.Value = pickerClientId;
            _isPickedUp.Value = NetworkObject.TrySetParent(clientNetworkObject);
        }

        public void OnDrop()
        {
            if (!canDrop) return;
            
            OnDropServerRpc();
        }
        
        [Rpc(SendTo.Server)]
        private void OnDropServerRpc()
        {
            if (NetworkObject.TryRemoveParent())
            {
                _isPickedUp.Value = false;
                _holderClientId.Value = default;
            }
        }

        public void DespawnKey()
        {
            OnDespawnKeyServerRpc();
        }

        [Rpc(SendTo.Server)]
        private void OnDespawnKeyServerRpc()
        {
            NetworkObject.Despawn();
        }
    }
}