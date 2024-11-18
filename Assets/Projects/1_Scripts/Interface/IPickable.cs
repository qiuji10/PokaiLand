using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PokaiLand
{
    public interface IPickable
    {
        bool IsPickedUp { get; }
        ulong HolderClientId { get; }
        InputAction PickAction { get; }
        void OnPick(ulong pickerClientId);
        void OnDrop();
    }

    public struct PickableInfo : INetworkSerializable
    {
        public bool isPickedUp;
        public ulong pickerClientId;

        public PickableInfo(bool isPickedUp, ulong pickerClientId)
        {
            this.isPickedUp = isPickedUp;
            this.pickerClientId = pickerClientId;
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref isPickedUp);
            serializer.SerializeValue(ref pickerClientId);
        }
    }
}