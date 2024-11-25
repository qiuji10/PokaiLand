using Unity.Netcode;

namespace PokaiLand.Events
{
    struct PlayerMovementEvent
    {
        public readonly float HorizontalVelocity;

        public PlayerMovementEvent(float horizontalVelocity)
        {
            this.HorizontalVelocity = horizontalVelocity;
        }
    }

    struct PlayerJumpEvent
    {
    }
    
    struct PlayerLandedEvent
    {
    }

    struct EnterDoorEvent : INetworkSerializable
    {
        public ulong ClientId; // client that enter the door

        public EnterDoorEvent(ulong clientId)
        {
            this.ClientId = clientId;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
        }
    }
}