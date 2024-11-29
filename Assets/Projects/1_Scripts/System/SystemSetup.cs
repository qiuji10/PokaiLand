using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PokaiLand.Enum;
using PokaiLand.GameMode;
using PokaiLand.Utility;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Infrastructure
{
    // public class RPCMethod<T>
    // {
    //     private UniTaskCompletionSource<T> _promise;
    //
    //     private UniTask<T> Run()
    //     {
    //         _promise = new UniTaskCompletionSource<T>();
    //         ServerSendClientNetworkObjectIdRpc();
    //         return _promise.Task;
    //     }
    //
    //     [Rpc(SendTo.Server, DeferLocal = true)]
    //     private void ServerSendClientNetworkObjectIdRpc()
    //     {
    //         ClientReceiveNetworkObjectIdRpc();
    //     }
    //
    //     [Rpc(SendTo.ClientsAndHost)]
    //     private void ClientReceiveNetworkObjectIdRpc(ulong networkObjectId)
    //     {
    //         _promise.TrySetResult();
    //     }
    // }
    
    public partial class SystemSetup : NetworkBehaviour
    {
        private readonly string _systemLabel = EAddressableLabels.System.ToString();
        private readonly IEnumerable<EAddressableLabels> _addressableLabels = System.Enum.GetValues(typeof(EAddressableLabels)).Cast<EAddressableLabels>();
        private readonly ConcurrentDictionary<Type, object> _systemCache = new ConcurrentDictionary<Type, object>();
        private readonly Dictionary<EAddressableLabels, ulong> _systemNetworkId = new Dictionary<EAddressableLabels, ulong>();

        public override async void OnNetworkSpawn()
        {
            _systemCache.Clear();
            
            var cameraSystem = CameraNetworkSystem.Instance ?? await CreateSystem<CameraNetworkSystem>(EAddressableLabels.Camera, ECameraType.Default);
            var networkManager = NetworkManager.Singleton ?? await CreateNativeSystem<NetworkManager>(EAddressableLabels.Network);
            await UniTask.WaitUntil(() => networkManager.IsClient || networkManager.IsServer);
                
            var gameModeManager = await CreateNetworkSystem<GameModeManager>(EAddressableLabels.GameMode, cameraSystem);
            
            try
            {
              
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
    }
}