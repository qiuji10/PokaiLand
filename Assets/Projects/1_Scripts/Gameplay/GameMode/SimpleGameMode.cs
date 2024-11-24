using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.GameMode
{
    using Player;
    
    [Serializable]
    public class SimpleGameMode : BaseGameMode
    {
        public SimpleGameMode(NetworkBehaviour networkBehaviour) : base(networkBehaviour)
        {
        }
        
        private NetworkList<FixedString32Bytes> _playerNames = new NetworkList<FixedString32Bytes>();

        public override void OnClientConnected(ulong clientId)
        {
            _playerNames.Add($"Player {clientId}");
            debugClientRpc();
            UpdatePlayerColor(clientId, ClientIds.IndexOf(clientId));
        }

        [Rpc(SendTo.ClientsAndHost)]
        private void debugClientRpc()
        {
            for (int i = 0; i < _playerNames.Count; i++)
            {
                Debug.Log(_playerNames[i]);
            }
        }

        private void UpdatePlayerColor(ulong clientId, int playerIndex)
        {
            var clientNetworkObject = NetworkManager.SpawnManager.GetPlayerNetworkObject(clientId);
            var playerDeco = clientNetworkObject?.GetComponent<PlayerDecorator>();
            if (playerDeco != null)
            {
                playerDeco.playerName.Value = $"Player {playerIndex}";
                playerDeco.playerColor.Value = GetPlayerColor(playerIndex);
            }
        }
        
        private Color GetPlayerColor(int playerIndex)
        {
            return playerIndex < _playerColors.Length 
                ? _playerColors[playerIndex] 
                : Color.gray; // fallback color
        }
        
        private readonly Color[] _playerColors = new[]
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.white,
            Color.black,
            // Add more colors as needed
        };
    }
}