using System;
using Cysharp.Threading.Tasks;
using PokaiLand.Enum;
using PokaiLand.GameMode.V1;
using PokaiLand.Gameplay.Map;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Infrastructure
{
    public partial class SystemSetup : NetworkBehaviour
    {
        public override async void OnNetworkSpawn()
        {
            _systemCache.Clear();
            
            var cameraSystem = CameraNetworkSystem.Instance ?? await CreateSystem<CameraNetworkSystem>(EAddressableLabels.Camera, ECameraType.Default);
            var gameModeManager = await CreateNetworkSystem<GameModeManager>(EAddressableLabels.GameMode, cameraSystem);
            var mapSystem = await CreateNetworkSystem<MapSystem>(EAddressableLabels.Map, "Map_01");

        }
    }
}