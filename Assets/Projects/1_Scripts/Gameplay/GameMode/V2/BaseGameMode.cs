using System;
using Unity.Netcode;

namespace PokaiLand.Gameplay.GameMode.V2
{
    public abstract class BaseGameMode : NetworkBehaviour
    {
        public abstract bool InitializedOnClientSide { get; }

        protected virtual void OnEnable()
        {
            if (!IsServer) return;
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
        
        protected virtual void OnDisable()
        {
            if (!IsServer) return;
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        protected virtual void OnClientConnected(ulong clientId)
        {
        }

        protected virtual void OnClientDisconnected(ulong clientId)
        {  
        }

        public abstract bool CanStartGame();
        public abstract void ServerStartGame();
        public abstract void ServerStopGame();
    }
}