using PokaiLand.Enum;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PokaiLand
{
    public interface IInteractable : IMapObject
    {
        EInteractable Type { get; }
        InputAction InputAction { get; }
        void Interact(InteractInfo info);
    }

    public struct InteractInfo : INetworkSerializable
    {
        public ulong ClientId;

        public InteractInfo(ulong clientId)
        {
            this.ClientId = clientId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
        }
    }
}