using System;
using System.Collections.Generic;
using PokaiLand.Enum;
using PokaiLand.Events;
using PokaiLand.Input;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PokaiLand.Item
{
    public class Door : NetworkBehaviour, IInteractable, IDisposable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite spOpen;
        [SerializeField] private Sprite spClose;

        public bool IsOpen => _isOpen.Value;
        public EAddressableLabels Labels => EAddressableLabels.Interactive | EAddressableLabels.Door;
        public EInteractable Type => EInteractable.Door;
        public InputAction InputAction => new PlayerControls().Player.Interact;
        
        private readonly NetworkVariable<bool> _isOpen = new();
        private readonly NetworkList<ulong> _enteredClientIds = new(new List<ulong>());

        public void OnEnable()
        {
            _isOpen.OnValueChanged += OnDoorStateChanged;
        }

        public void OnDisable()
        {
            _isOpen.OnValueChanged -= OnDoorStateChanged;
        }

        public void Interact(InteractInfo info)
        {
            if (!IsOpen)
                ServerOpenDoorRpc();
            else
            {
                ServerInteractDoorRpc(info);
            }
        }

        [Rpc(SendTo.Server, DeferLocal = true)]
        private void ServerOpenDoorRpc()
        {
            _isOpen.Value = true;
        }

        [Rpc(SendTo.Server, DeferLocal = true)]
        private void ServerInteractDoorRpc(InteractInfo info)
        {
            ServerEnterDoorEvent param;
            
            if (!_enteredClientIds.Contains(info.ClientId))
            {
                _enteredClientIds.Add(info.ClientId);
                param = new ServerEnterDoorEvent(info.ClientId, true);
            }
            else
            {
                _enteredClientIds.Remove(info.ClientId);
                param = new ServerEnterDoorEvent(info.ClientId, false);
            }
            
            EventBus.Execute(param);
            ClientInteractDoorRpc(new ClientEnterDoorEvent(param));
        }

        [Rpc(SendTo.ClientsAndHost, DeferLocal = true)]
        private void ClientInteractDoorRpc(ClientEnterDoorEvent e)
        {
            EventBus.Execute(e);
        }

        private void OnDoorStateChanged(bool previousValue, bool newValue)
        {
            spriteRenderer.sprite = newValue ? spOpen : spClose;
        }

        public void Dispose()
        {
            _isOpen?.Dispose();
            _enteredClientIds?.Dispose();
        }
    }
}
