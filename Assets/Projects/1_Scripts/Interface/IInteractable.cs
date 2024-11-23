using PokaiLand.Enum;
using UnityEngine.InputSystem;

namespace PokaiLand
{
    public interface IInteractable
    {
        EInteractable Type { get; }
        InputAction InputAction { get; }
    }
}