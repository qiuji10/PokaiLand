using UnityEngine;
using UnityEngine.InputSystem;

namespace PokaiLand
{
    public interface IPickable : IInteractable
    {
        bool IsPickedUp { get; }
        bool CanDrop { get; }
        ulong HolderClientId { get; }
        void OnPick(ulong pickerClientId);
        void OnDrop();
    }
}