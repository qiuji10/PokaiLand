using Unity.Netcode;

namespace PokaiLand.Utility
{
    public static class NetworkUtils
    {
        public static NetworkObject GetNetworkObject(ulong id)
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var clientNetworkObject);
            return clientNetworkObject;
        }
    }
}