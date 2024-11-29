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
    public partial class SystemSetup
    {
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
                GameObject prefab = await SystemUtility.FindAsset<GameObject>(array);
                var networkObject = prefab.GetComponent<NetworkObject>().InstantiateAndSpawn(NetworkManager);

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
                Debug.Log("start to await");
                NetworkObject networkSystemObj = await FindNetworkObject(labels);
                Debug.Log("end await");
                
                if (networkSystemObj.TryGetComponent(out T component))
                {
                    await component.Init(args);
                    _systemCache.TryAdd(typeof(T), component);
                    _systemNetworkId.TryAdd(labels, networkSystemObj.NetworkObjectId);
                    return component;
                }
                
                throw new NullReferenceException($"Can't find network object of type {typeof(T).Name} on client");
            }

            throw new Exception($"Server or client connection not initiated");
        }

        private UniTaskCompletionSource<NetworkObject> _findNetworkObjectPromise;

        private UniTask<NetworkObject> FindNetworkObject(EAddressableLabels labels)
        {
            _findNetworkObjectPromise = new UniTaskCompletionSource<NetworkObject>();
            Debug.Log("calling server rpc");
            ServerSendClientNetworkObjectIdRpc(NetworkManager.LocalClientId, labels);
            return _findNetworkObjectPromise.Task;
        }

        [Rpc(SendTo.Server, DeferLocal = true)]
        private void ServerSendClientNetworkObjectIdRpc(ulong senderClientId, EAddressableLabels labels)
        {
            Debug.Log("server receive rpc call, calling client rpc");
            
            if (!_systemNetworkId.ContainsKey(labels))
                _findNetworkObjectPromise.TrySetException(new Exception($"labels not found in _systemNetworkId: {labels}"));

            ClientReceiveNetworkObjectIdRpc(_systemNetworkId[labels], RpcTarget.Single(senderClientId, RpcTargetUse.Temp));
        }

        [Rpc(SendTo.SpecifiedInParams, DeferLocal = true)]
        private void ClientReceiveNetworkObjectIdRpc(ulong networkObjectId, RpcParams prams)
        {
            Debug.Log("clien receive rpc call, setting result");
            
            if (!NetworkManager.SpawnManager.SpawnedObjects.ContainsKey(networkObjectId))
                _findNetworkObjectPromise.TrySetException(new Exception($"networkObjectId not found in SpawnedObjects: {networkObjectId}"));
            
            _findNetworkObjectPromise.TrySetResult(NetworkManager.SpawnManager.SpawnedObjects[networkObjectId]);
        }
        
        public IEnumerable<T> FindNetworkSystem<T>() where T : NetworkBehaviour, INetworkSystem<T>
        {
            return _systemCache.Values
                .OfType<T>()
                .Where(system => system.GetType() == typeof(INetworkSystem<T>));
        }
    }
}