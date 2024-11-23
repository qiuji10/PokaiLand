using UnityEngine;
using UnityEngine.InputSystem;

namespace PokaiLand
{
    public interface IPickable
    {
        bool IsPickedUp { get; }
        bool CanDrop { get; }
        ulong HolderClientId { get; }
        InputAction InputAction { get; }
        void OnPick(ulong pickerClientId);
        void OnDrop();
    }
}