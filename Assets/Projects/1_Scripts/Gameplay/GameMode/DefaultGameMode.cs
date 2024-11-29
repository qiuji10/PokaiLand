using System;
using System.Collections.Generic;
using PokaiLand.Enum;
using PokaiLand.Events;
using PokaiLand.Network;
using PokaiLand.Utility;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.GameMode
{
    using Player;
    
    [Serializable]
    public class DefaultGameMode : BaseGameMode
    {
        private NetworkList<FixedString32Bytes> _playerNames = new();
        
        private readonly Color[] _colorIndexes = new[]
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
        };
        
        private int _playerCompleteCount;
        
        public override ECameraType CameraType => ECameraType.Default;
        private const string ENTER_DOOR_MESSAGE = "ENTER_DOOR";
        private const string END_GAME_MESSAGE = "END_GAME";
        
        public DefaultGameMode(NetworkBehaviour networkBehaviour, CameraNetworkSystem cameraNetworkSystem) : base(networkBehaviour, cameraNetworkSystem)
        {
            EventBus.Register<EnterDoorEvent>(OnEnterDoorEvent);
            
            
            NetworkMessage.Register<EnterDoorEvent>(ENTER_DOOR_MESSAGE, HandleEnterDoorMessage);
            NetworkMessage.Register(END_GAME_MESSAGE, HandleEndGameMessage);

            if (NetworkManager.IsServer)
            {
                NetworkManager.OnClientConnectedCallback += InitPlayer;
                NetworkManager.OnClientConnectedCallback += InitPlayer;
                
                InitPlayer(NetworkManager.LocalClientId);
            }
        }

        protected override void OnDeconstructGameMode()
        {
            base.OnDeconstructGameMode();
            EventBus.Deregister<EnterDoorEvent>(OnEnterDoorEvent);
            
            NetworkMessage.Unregister(ENTER_DOOR_MESSAGE);
            NetworkMessage.Unregister(END_GAME_MESSAGE);
            
            if (NetworkManager.IsServer)
            {
                NetworkManager.OnClientConnectedCallback -= InitPlayer;
                NetworkManager.OnClientConnectedCallback -= InitPlayer;
            }
        }

        public override void OnServerInit()
        {

        }

        private void InitPlayer(ulong clientId)
        {
            _playerNames.Add($"Player {clientId}");
            int index = NetworkManager.Singleton.ConnectedClientsList.Count - 1;
            UpdatePlayerColor(clientId, index);
        }
        
        private void OnEnterDoorEvent(EnterDoorEvent e)
        {
            Debug.Log($"OnEnterDoorEvent triggered with ClientId: {e.ClientId}");
            
            // Send message to server
            NetworkMessage.SendToServer(ENTER_DOOR_MESSAGE, e);
        }

        private void HandleEnterDoorMessage(ulong senderId, EnterDoorEvent e)
        {
            if (!NetworkManager.IsServer) return;
            
            Debug.Log($"HandleEnterDoorMessage received on server for ClientId: {e.ClientId}");
            
            var clientNetworkObj = NetworkUtils.GetNetworkObject(e.ClientId);
            if (clientNetworkObj == null) return;

            if (clientNetworkObj.IsSpawned)
            {
                _playerCompleteCount++;

            
                clientNetworkObj.Despawn();

                if (_playerCompleteCount >= _playerNames.Count)
                {
                    SendEndGame();
                }
            }
        }
        
        private void SendEndGame()
        {
            // Create an empty writer since we don't need to send any data
            NetworkMessage.SendToAll(END_GAME_MESSAGE);
        }

        private void HandleEndGameMessage(ulong senderId)
        {
            Debug.Log("Game Ended");
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
            return playerIndex < _colorIndexes.Length ? _colorIndexes[playerIndex] : Color.gray;
        }
    }
}