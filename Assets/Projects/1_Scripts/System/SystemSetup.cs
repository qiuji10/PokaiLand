using System;
using PokaiLand.Enum;
using PokaiLand.Gameplay.GameMode.V2;
using PokaiLand.Gameplay.Map;
using PokaiLand.Player;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Infrastructure
{
    public partial class SystemSetup : NetworkBehaviour
    {
        public override async void OnNetworkSpawn()
        {
            _systemCache.Clear();
            
            var cameraSystem = 
                CameraNetworkSystem.Instance 
                ? await CameraNetworkSystem.Instance.Init(ECameraType.Default) 
                : await CreateNetworkSystem<CameraNetworkSystem>(EAddressableLabels.Camera, ECameraType.Default);
            
            var mapSystem = await CreateNetworkSystem<MapSystem>(EAddressableLabels.Map, "Map_01");
            var playerSystem = await CreateNetworkSystem<PlayerSystem>(EAddressableLabels.Player);
            var gameModeManager = await CreateNetworkSystem<DefaultGameMode>(EAddressableLabels.GameMode, cameraSystem);
        }
    }
}