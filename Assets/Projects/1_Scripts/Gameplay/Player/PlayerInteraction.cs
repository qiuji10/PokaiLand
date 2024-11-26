using System.Collections.Generic;
using PokaiLand.Enum;
using PokaiLand.Events;
using PokaiLand.Item;
using PokaiLand.Utilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using Key = PokaiLand.Item.Key;

namespace PokaiLand.Player
{
    using Input;

    public class PlayerInteraction : NetworkBehaviour
    {
        [SerializeField] private Collider2D collider2d;
        private PlayerControls _playerControls;
        private IPickable _currentHoldingPickable;
        private CollisionHandler<IPickable> _pickableHandler;
        private CollisionHandler<IInteractable> _interactableHandler;

        private void Awake()
        {
            _playerControls = new PlayerControls();
            
            _pickableHandler = new CollisionHandler<IPickable>(
                transform,
                collider2d,
                pickable => ((MonoBehaviour)pickable).transform.position,
                pickable => !pickable.IsPickedUp
            );

            _interactableHandler = new CollisionHandler<IInteractable>(
                transform,
                collider2d,
                interactable => ((MonoBehaviour)interactable).transform.position
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
            _playerControls.Player.Interact.performed += OnPlayerInteractAttempt;
        }

        private void OnDisable()
        {
            if (_playerControls == null) return;

            _playerControls.Player.Interact.performed -= OnPlayerInteractAttempt;
            _playerControls.Disable();
        }

        private void OnPlayerInteractAttempt(InputAction.CallbackContext ctx)
        {
            if (_interactableHandler.CurrentTarget != null)
            {
                bool inDoorRange = _interactableHandler.CurrentTarget is { Type: EInteractable.Door };
                bool holdingKey = _currentHoldingPickable is { Type: EInteractable.Key };
                
                if (inDoorRange && holdingKey)
                {
                    _interactableHandler.CurrentTarget.Interact(new InteractInfo(NetworkObjectId));
                    ((Key)_currentHoldingPickable).DespawnKey();
                    _currentHoldingPickable = null;
                    return;
                }

                if (inDoorRange && ((Door)_interactableHandler.CurrentTarget).IsOpen)
                {
                    _interactableHandler.CurrentTarget.Interact(new InteractInfo(NetworkObjectId));
                    return;
                }
            }
            
            if (_currentHoldingPickable != null)
            {
                if (_currentHoldingPickable.CanDrop)
                {
                    _currentHoldingPickable.OnDrop();
                    _currentHoldingPickable = null;
                }
            }
            else
            {
                if (_pickableHandler.CurrentTarget is { IsPickedUp: false })
                {
                    _currentHoldingPickable = _pickableHandler.CurrentTarget;
                    _currentHoldingPickable.OnPick(NetworkObjectId);
                }
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
