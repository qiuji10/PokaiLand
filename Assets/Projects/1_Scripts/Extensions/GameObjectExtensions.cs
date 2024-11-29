using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Extensions
{
    public static class GameObjectExtensions
    {
        public static NetworkObject GetNetworkObject(this GameObject gameObject)
        {
            return gameObject.GetComponent<NetworkObject>();
        }
        
        public static NetworkObject SpawnOnNetwork(this GameObject gameObject, bool destroyWithScene = true)
        {
            var networkObject = gameObject.GetComponent<NetworkObject>();
            networkObject.Spawn(destroyWithScene);
            return networkObject; 
        }
    }
}