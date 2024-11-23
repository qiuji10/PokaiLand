using System.Collections.Generic;
using PokaiLand.Player.PokaiLand.Utilities;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PokaiLand.Player
{
    using Input;

    public class PlayerInteraction : NetworkBehaviour
    {
        private PlayerControls _playerControls;
        private IPickable _currentHoldingPickable;
        private CollisionHandler<IPickable> _pickableHandler;
        private CollisionHandler<IInteractable> _interactableHandler;

        private void Awake()
        {
            _playerControls = new PlayerControls();
            
            _pickableHandler = new CollisionHandler<IPickable>(
                transform,
                pickable => ((MonoBehaviour)pickable).transform.position,
                pickable => !pickable.IsPickedUp
            );

            _interactableHandler = new CollisionHandler<IInteractable>(
                transform,
                interactable => ((MonoBehaviour)interactable).transform.position,
                interactable => true // Add conditions for valid interactables if needed
            );
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
            _playerControls.Player.Interact.performed += OnPlayerPickAttempt;
        }

        private void OnDisable()
        {
            if (_playerControls == null) return;

            _playerControls.Player.Interact.performed -= OnPlayerPickAttempt;
            _playerControls.Disable();
        }

        private void OnPlayerPickAttempt(InputAction.CallbackContext ctx)
        {
            if (_currentHoldingPickable != null && _currentHoldingPickable.CanDrop)
            {
                _currentHoldingPickable.OnDrop();
                _currentHoldingPickable = null;
                return;
            }
            
            if (_pickableHandler.CurrentTarget == null) return;
            
            var pickable = _pickableHandler.CurrentTarget;
            
            if (!pickable.IsPickedUp)
            {
                _currentHoldingPickable = pickable;
                pickable.OnPick(NetworkObject.NetworkObjectId);
            }
        }
        
        private void OnTriggerEnter2D(Collider2D col)
        {
            _pickableHandler.OnTriggerEnter(col);
            _interactableHandler.OnTriggerEnter(col);
        }

        private void OnTriggerExit2D(Collider2D col)
        {
            _pickableHandler.OnTriggerExit(col);
            _interactableHandler.OnTriggerExit(col);
        }
    }
}
