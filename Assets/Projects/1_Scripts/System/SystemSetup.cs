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
    public static partial class SystemSetup
    {
        private static readonly ConcurrentDictionary<Type, object> SystemCache = new ConcurrentDictionary<Type, object>();
        private static readonly string SystemLabel = EAddressableLabels.System.ToString();
        private static readonly IEnumerable<EAddressableLabels> AddressableLabels = System.Enum.GetValues(typeof(EAddressableLabels)).Cast<EAddressableLabels>();
        
        [RuntimeInitializeOnLoadMethod]
        public static async void Initialize()
        {
            SystemCache.Clear();
            
            try
            {
                var cameraSystem = CameraNetworkSystem.Instance ?? await CreateSystem<CameraNetworkSystem>(EAddressableLabels.Camera, ECameraType.Default);
                var networkManager = NetworkManager.Singleton ?? await CreateNativeSystem<NetworkManager>(EAddressableLabels.Network);
                await UniTask.WaitUntil(() => networkManager.IsClient || networkManager.IsServer);
                
                var gameModeManager = await CreateNetworkSystem<GameModeManager>(EAddressableLabels.GameMode, cameraSystem);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}