using System;
using Cysharp.Threading.Tasks;
using PokaiLand.Enum;
using PokaiLand.Gameplay.GameMode.V2;
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
            var gameModeManager = await CreateNetworkSystem<DefaultGameMode>(EAddressableLabels.GameMode, cameraSystem);
            var mapSystem = await CreateNetworkSystem<MapSystem>(EAddressableLabels.Map, "Map_01");

        }
    }
}