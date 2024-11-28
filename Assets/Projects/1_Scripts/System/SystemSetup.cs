using System;
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
    public static class SystemSetup
    {
        private static readonly string SystemLabel = EAddressableLabels.System.ToString();
        private static readonly IEnumerable<EAddressableLabels> AddressableLabels = System.Enum.GetValues(typeof(EAddressableLabels)).Cast<EAddressableLabels>();
        
        //[RuntimeInitializeOnLoadMethod]
        public static async void Initialize()
        {
            try
            {
                var networkManager = NetworkManager.Singleton ?? await CreateSystem<NetworkManager>(EAddressableLabels.Network);
                var cameraSystem = CameraSystem.Instance ?? await CreateSystem<CameraSystem>(EAddressableLabels.Camera);
                var gameModeManager = await CreateSystem<GameModeManager>(EAddressableLabels.GameMode);
                //await UniTask.WaitUntil(() => networkManager.IsServer || networkManager.IsClient);

                gameModeManager.Init(cameraSystem);
                await cameraSystem.Init(gameModeManager.GameMode.CameraType);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private static async UniTask<T> CreateSystem<T>(EAddressableLabels labels)
        {
            var array = AddressableLabels
                .Where(l => labels.HasFlag(l))
                .Select(l => l.ToString())
                .Concat(new[] { SystemLabel })
                .ToArray();
            
            return await SystemUtility.CreateFromAsset<T>(array);
        }
        
        // private static async UniTask<T> CreateNetworkSystem<T>(EAddressableLabels labels) where T : MonoBehaviour
        // {
        //     var array = AddressableLabels
        //         .Where(l => labels.HasFlag(l))
        //         .Select(l => l.ToString())
        //         .Concat(new[] { SystemLabel })
        //         .ToArray();
        //
        //     T prefab = await SystemUtility.FindAsset<T>(array);
        //     if (prefab.TryGetComponent(out NetworkObject networkObject))
        //     {
        //         return networkObject.InstantiateAndSpawn(NetworkManager.Singleton).GetComponent<T>();
        //     }
        //     
        //     throw new NullReferenceException($"Can't find network object of type {typeof(T).Name}");
        // }
        
        private static async UniTask<T> FindAsset<T>(EAddressableLabels labels)
        {
            var array = AddressableLabels
                .Where(l => labels.HasFlag(l))
                .Select(l => l.ToString())
                .Concat(new[] { SystemLabel })
                .ToArray();
            
            return await SystemUtility.FindAsset<T>(array);
        }
    }
}