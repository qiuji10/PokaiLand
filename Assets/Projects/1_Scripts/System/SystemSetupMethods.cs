using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PokaiLand.Enum;
using PokaiLand.Utility;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Infrastructure
{
    public static partial class SystemSetup
    {
        private static async UniTask<T> CreateSystem<T>(EAddressableLabels labels, params object[] args) where T : MonoBehaviour, ISystem<T>
        {
            var array = AddressableLabels
                .Where(l => labels.HasFlag(l))
                .Select(l => l.ToString())
                .Concat(new[] { SystemLabel })
                .ToArray();
            
            var systemComponent = await SystemUtility.CreateFromAsset<T>(array);
            
            Debug.Log($"creating {systemComponent}");
            
            if (systemComponent.TryGetComponent(out ISystem<T> system))
            {
                SystemCache.TryAdd(typeof(T), systemComponent);
                await system.Init(args);
                return systemComponent;
            }
            
            throw new NullReferenceException($"Can't find system object of type {typeof(T).Name}");
        }
        
        private static async UniTask<T> CreateNativeSystem<T>(EAddressableLabels labels, params object[] args) where T : MonoBehaviour
        {
            var array = AddressableLabels
                .Where(l => labels.HasFlag(l))
                .Select(l => l.ToString())
                .Concat(new[] { SystemLabel })
                .ToArray();

            var system = await SystemUtility.CreateFromAsset<T>(array);
            SystemCache.TryAdd(typeof(T), system);
            return system;
        }
        
        private static async UniTask<T> CreateNetworkSystem<T>(EAddressableLabels labels, params object[] args) where T : NetworkBehaviour, INetworkSystem<T>
        {
            var array = AddressableLabels
                .Where(l => labels.HasFlag(l))
                .Select(l => l.ToString())
                .Concat(new[] { SystemLabel })
                .ToArray();
        
            GameObject prefab = NetworkManager.Singleton.IsServer ? await SystemUtility.FindAsset<GameObject>(array) : GameObject.FindFirstObjectByType<T>().gameObject;
            
            Debug.Log($"prefab is {prefab}");
            
            if (prefab.TryGetComponent(out INetworkSystem<T> system))
            {
                await system.Init(args);
                var networkObject = prefab.GetComponent<NetworkObject>().InstantiateAndSpawn(NetworkManager.Singleton);
                var component = networkObject.GetComponent<T>();
                SystemCache.TryAdd(typeof(T), component);
                return component;
            }

            Debug.Log($"{labels} is null");

            throw new NullReferenceException($"Can't find network object of type {typeof(T).Name}");
        }
        
        public static IEnumerable<T> FindNetworkSystem<T>() where T : NetworkBehaviour, INetworkSystem<T>
        {
            return SystemCache.Values
                .OfType<T>()
                .Where(system => system.GetType() == typeof(INetworkSystem<T>));
        }
    }
}