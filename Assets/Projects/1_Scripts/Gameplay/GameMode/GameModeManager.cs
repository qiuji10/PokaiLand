using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.GameMode
{
    public class GameModeManager : NetworkBehaviour
    {
        [SerializeReference, SubclassSelector] BaseGameMode gameMode;

        private void Awake()
        {
            var type = gameMode.GetType();
            gameMode = Activator.CreateInstance(type, new object[] { this }) as BaseGameMode;
        }

        public override void OnNetworkSpawn()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        
        public override void OnNetworkDespawn()
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (IsServer)
            {
                gameMode.AddClientToIdList(clientId);
                gameMode.OnClientConnected(clientId);
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (IsServer)
            {
                gameMode.RemoveClientFromIdList(clientId);
                gameMode.OnClientDisconnected(clientId);
            }
        }
    }
}
