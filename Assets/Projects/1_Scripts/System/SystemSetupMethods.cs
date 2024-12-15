using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PokaiLand.Enum;
using PokaiLand.Utility;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Infrastructure
{
    public partial class SystemSetup
    {
        private readonly RpcHandler<NetworkObject> _findNetworkObjectHandler = new();
     
        private readonly string _systemLabel = EAddressableLabels.System.ToString();
        private readonly IEnumerable<EAddressableLabels> _addressableLabels = System.Enum.GetValues(typeof(EAddressableLabels)).Cast<EAddressableLabels>();
        private readonly ConcurrentDictionary<Type, object> _systemCache = new ConcurrentDictionary<Type, object>();
        private readonly Dictionary<EAddressableLabels, ulong> _systemNetworkId = new Dictionary<EAddressableLabels, ulong>();
        
        private async UniTask<T> CreateSystem<T>(EAddressableLabels labels, params object[] args) where T : MonoBehaviour, ISystem<T>
        {
            var array = _addressableLabels
                .Where(l => labels.HasFlag(l))
                .Select(l => l.ToString())
                .Concat(new[] { _systemLabel })
                .ToArray();
            
            var systemComponent = await SystemUtility.CreateFromAsset<T>(array);
            
            Debug.Log($"creating {systemComponent}");
            
            if (systemComponent.TryGetComponent(out ISystem<T> system))
            {
                _systemCache.TryAdd(typeof(T), systemComponent);
                await system.Init(args);
                return systemComponent;
            }
            
            throw new NullReferenceException($"Can't find system object of type {typeof(T).Name}");
        }
        
        private async UniTask<T> CreateNativeSystem<T>(EAddressableLabels labels, params object[] args) where T : MonoBehaviour
        {
            var array = _addressableLabels
                .Where(l => labels.HasFlag(l))
                .Select(l => l.ToString())
                .Concat(new[] { _systemLabel })
                .ToArray();

            var system = await SystemUtility.CreateFromAsset<T>(array);
            _systemCache.TryAdd(typeof(T), system);
            return system;
        }
        
        private async UniTask<T> CreateNetworkSystem<T>(EAddressableLabels labels, params object[] args) where T : NetworkBehaviour, INetworkSystem<T>
        {
            var array = _addressableLabels
                .Where(l => labels.HasFlag(l))
                .Select(l => l.ToString())
                .Concat(new[] { _systemLabel })
                .ToArray();

            if (NetworkManager.Singleton.IsServer)
            {
                GameObject prefab = await SystemUtility.FindAssetByLabels<GameObject>(array);
                var networkObject = prefab.GetComponent<NetworkObject>().InstantiateAndSpawn(NetworkManager);
                networkObject.gameObject.name = prefab.name;

                if (networkObject.TryGetComponent(out T component))
                {
                    await component.Init(args);
                    _systemCache.TryAdd(typeof(T), component);
                    _systemNetworkId.TryAdd(labels, networkObject.NetworkObjectId);
                    return component;
                }
                
                throw new NullReferenceException($"Can't find network object of type {typeof(T).Name} on server");
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                NetworkObject networkSystemObj = await _findNetworkObjectHandler.Run(() => ServerSendClientNetworkObjectIdRpc(NetworkManager.LocalClientId, labels));
                
                if (networkSystemObj.TryGetComponent(out T component))
                {
                    if (component.InitializedOnClientSide)
                        await component.Init(args);
                    
                    _systemCache.TryAdd(typeof(T), component);
                    _systemNetworkId.TryAdd(labels, networkSystemObj.NetworkObjectId);
                    return component;
                }
                
                throw new NullReferenceException($"Can't find network object of type {typeof(T).Name} on client");
            }

            throw new Exception($"Server or client connection not initiated");
        }

        [Rpc(SendTo.Server, DeferLocal = true)]
        private void ServerSendClientNetworkObjectIdRpc(ulong senderClientId, EAddressableLabels labels)
        {
            if (!_systemNetworkId.ContainsKey(labels))
                _findNetworkObjectHandler.SetException(new Exception($"labels not found in _systemNetworkId: {labels}"));

            ClientReceiveNetworkObjectIdRpc(_systemNetworkId[labels], RpcTarget.Single(senderClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams, DeferLocal = true)]
        private void ClientReceiveNetworkObjectIdRpc(ulong networkObjectId, RpcParams prams)
        {
            if (!NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(networkObjectId))
                _findNetworkObjectHandler.SetException(new Exception($"networkObjectId not found in SpawnedObjects: {networkObjectId}"));

            _findNetworkObjectHandler.SetResult(NetworkManager.SpawnManager.SpawnedObjects[networkObjectId]);
        }
        
        public IEnumerable<T> FindNetworkSystem<T>() where T : NetworkBehaviour, INetworkSystem<T>
        {
            return _systemCache.Values
                .OfType<T>()
                .Where(system => system.GetType() == typeof(INetworkSystem<T>));
        }
    }
}