using System;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PokaiLand.Player
{
    using Input;
    
    public class PlayerInteraction : NetworkBehaviour
    {
        private IPickable _pickable;
        private PlayerControls _playerControls;

        private void Awake()
        {
            _playerControls = new PlayerControls();
        }

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
                this.enabled = false;
        }

        private void OnEnable()
        {
            if (_playerControls == null) return;
            
            _playerControls.Enable();
            _playerControls.Player.Pick.performed += OnPlayerPickAttempt;
        }

        private void OnDisable()
        {
            if (_playerControls == null) return;
            
            _playerControls.Player.Pick.performed -= OnPlayerPickAttempt;
            _playerControls.Disable();
        }
        
        private void OnPlayerPickAttempt(InputAction.CallbackContext ctx)
        {
            if (_pickable != null)
            {
                if (!_pickable.IsPickedUp)
                    _pickable.OnPick(NetworkObject.NetworkObjectId);
                else
                    _pickable.OnDrop();
            }
        }
        
        void OnTriggerEnter2D(Collider2D col)
        {
            if (_pickable == null && col.TryGetComponent(out IPickable pickable) && !pickable.IsPickedUp)
            {
                Debug.Log($"{gameObject.name} entered pickable");
                _pickable = pickable;
            }
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            if (_pickable != null && !_pickable.IsPickedUp && col.TryGetComponent(out IPickable pickable) && _pickable == pickable)
            {
                Debug.Log($"{gameObject.name} exited pickable");
                _pickable = null;
            }
        }
    }
}