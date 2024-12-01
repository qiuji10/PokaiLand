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
            ServerOnClientGameModeSpawnRpc(NetworkManager.LocalClientId);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            ServerOnClientGameModeDespawnRpc(NetworkManager.LocalClientId);
        }

        protected virtual void ServerOnClientGameModeSpawnRpc(ulong clientId)
        {
        }

        protected virtual void ServerOnClientGameModeDespawnRpc(ulong clientId)
        {
        }

        public abstract bool CanStartGame();
        public abstract void ServerStartGame();
        public abstract void ServerStopGame();
    }
}