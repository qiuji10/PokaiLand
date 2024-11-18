using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.GameMode
{
    public class GameModeManager : NetworkBehaviour
    {
        [SerializeReference, SubclassSelector] BaseGameMode gameMode;
        private static GameModeManager Instance { get; set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);
            
            DontDestroyOnLoad(gameObject);
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
            gameMode.AddClientToIdList(clientId);
            
            if (IsServer)
                gameMode.OnClientConnected(clientId);
        }

        private void OnClientDisconnected(ulong clientId)
        {
            gameMode.RemoveClientFromIdList(clientId);
            
            if (IsServer)
                gameMode.OnClientDisconnected(clientId);
        }
    }
}
