using System;
using System.Collections.Generic;
using UnityEngine;

namespace PokaiLand.GameMode
{
    using Player;
    
    [Serializable]
    public class SampleBaseGameMode : BaseGameMode
    {
        public override void OnClientConnected(ulong clientId)
        {
            UpdatePlayerColor(clientId, ClientIds.IndexOf(clientId));
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
        
        private static Color GetPlayerColor(int playerIndex)
        {
            switch (playerIndex)
            {
                case 0: return Color.red;
                case 1: return Color.green;
                case 2: return Color.blue;
                case 3: return Color.white;
                default: return Color.black;
            }
        }
    }
}