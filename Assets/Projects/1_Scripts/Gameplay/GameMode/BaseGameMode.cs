using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace PokaiLand.Gameplay.GameMode.V2
{
    public abstract class BaseGameMode : NetworkBehaviour
    {
        public abstract bool InitializedOnClientSide { get; }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
        }

        public abstract bool CanStartGame();
        public abstract void ServerStartGame();
        public abstract void ServerStopGame();
    }
}