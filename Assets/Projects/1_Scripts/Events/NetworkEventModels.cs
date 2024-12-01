using Unity.Netcode;

namespace PokaiLand.Events
{
    struct ClientEnterDoorEvent : INetworkSerializable
    {
        public ulong ClientId; // client that interact the door
        public bool IsEnterDoor;

        public ClientEnterDoorEvent(ServerEnterDoorEvent e)
        {
            ClientId = e.ClientId;
            IsEnterDoor = e.IsEnterDoor;
        }
        
        public ClientEnterDoorEvent(ulong clientId, bool isEnterDoor)
        {
            ClientId = clientId;
            IsEnterDoor = isEnterDoor;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref IsEnterDoor);
        }
    }
    
    struct ServerEnterDoorEvent : INetworkSerializable
    {
        public ulong ClientId; // client that interact the door
        public bool IsEnterDoor;

        public ServerEnterDoorEvent(ClientEnterDoorEvent e)
        {
            ClientId = e.ClientId;
            IsEnterDoor = e.IsEnterDoor;
        }
        
        public ServerEnterDoorEvent(ulong clientId, bool isEnterDoor)
        {
            ClientId = clientId;
            IsEnterDoor = isEnterDoor;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref IsEnterDoor);
        }
    }
}