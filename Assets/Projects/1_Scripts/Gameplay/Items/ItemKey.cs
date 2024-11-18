using NUnit.Framework.Constraints;
using PokaiLand.Input;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PokaiLand.Item
{
    public class ItemKey : NetworkBehaviour, IPickable
    {
        private readonly NetworkVariable<bool> _isPickedUp = new();
        private readonly NetworkVariable<ulong> _holderClientId = new();

        public bool IsPickedUp => _isPickedUp.Value;
        public ulong HolderClientId => _holderClientId.Value;

        public InputAction PickAction => new PlayerControls().Player.Pick;
        
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
    }
}