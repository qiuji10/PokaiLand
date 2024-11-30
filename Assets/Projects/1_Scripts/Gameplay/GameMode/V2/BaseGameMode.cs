using System;
using System.Collections.Generic;
using Unity.Netcode;

namespace PokaiLand.Gameplay.GameMode.V2
{
    public abstract class BaseGameMode : NetworkBehaviour
    {
        public abstract bool InitializedOnClientSide { get; }

        protected virtual void OnEnable()
        {
            if (!IsServer) return;
            NetworkManager.OnClientConnectedCallback += ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback += ServerOnClientDisconnected;
        }
        
        protected virtual void OnDisable()
        {
            if (!IsServer) return;
            NetworkManager.OnClientConnectedCallback -= ServerOnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= ServerOnClientDisconnected;
        }
        
        protected virtual void ServerOnClientConnected(ulong clientId)
        {
        }

        protected virtual void ServerOnClientDisconnected(ulong clientId)
        {
        }

        public abstract bool CanStartGame();
        public abstract void ServerStartGame();
        public abstract void ServerStopGame();
    }
}