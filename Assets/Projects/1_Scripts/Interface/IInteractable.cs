using PokaiLand.Enum;
using Unity.Netcode;
using UnityEngine.InputSystem;

namespace PokaiLand
{
    public interface IInteractable
    {
        NetworkObject NetworkObject { get; }
        EInteractable Type { get; }
        InputAction InputAction { get; }
        void Interact(InteractInfo info);
    }

    public struct InteractInfo
    {
        public readonly ulong ClientId;

        public InteractInfo(ulong clientId)
        {
            this.ClientId = clientId;
        }
    }
}