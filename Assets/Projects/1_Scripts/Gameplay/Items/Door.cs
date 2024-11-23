using PokaiLand.Enum;
using PokaiLand.Input;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PokaiLand.Item
{
    public class Door : NetworkBehaviour, IInteractable
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Sprite spOpen;
        [SerializeField] private Sprite spClose;

        public EInteractable Type => EInteractable.Door;
        public InputAction InputAction => new PlayerControls().Player.Interact;
    
        private readonly NetworkVariable<bool> _isOpen = new();

        public void OnEnable()
        {
            _isOpen.OnValueChanged += OnDoorStateChanged;
        }

        public void OnDisable()
        {
            _isOpen.OnValueChanged -= OnDoorStateChanged;
        }

        private void OnDoorStateChanged(bool previousValue, bool newValue)
        {
            spriteRenderer.sprite = newValue ? spOpen : spClose;
        }
    }
}
