using System;
using System.Collections.Generic;
using PokaiLand.Events;
using PokaiLand.Utilities;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.GameMode
{
    using Player;
    
    [Serializable]
    public class SimpleGameMode : BaseGameMode
    {
        private const string ENTER_DOOR_MESSAGE = "ENTER_DOOR";
        private const string END_GAME_MESSAGE = "END_GAME";
        
        public SimpleGameMode(NetworkBehaviour networkBehaviour) : base(networkBehaviour)
        {
            EventBus.Register<EnterDoorEvent>(OnEnterDoorEvent);
        }

        protected override void OnDeconstructGameMode()
        {
            base.OnDeconstructGameMode();
            EventBus.Deregister<EnterDoorEvent>(OnEnterDoorEvent);
            
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(ENTER_DOOR_MESSAGE);
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(END_GAME_MESSAGE);
        }

        public override void OnNetworkStart()
        {
            base.OnNetworkStart();
            // Register message handlers
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(ENTER_DOOR_MESSAGE, HandleEnterDoorMessage);
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(END_GAME_MESSAGE, HandleEndGameMessage);
        }

        private NetworkList<FixedString32Bytes> _playerNames = new();
        private int _playerCompleteCount;
        
        private readonly Color[] _playerColors = new[]
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
        };

        public override void OnClientConnected(ulong clientId)
        {
            _playerNames.Add($"Player {clientId}");
            UpdatePlayerColor(clientId, ClientIds.IndexOf(clientId));
        }
        
        private void OnEnterDoorEvent(EnterDoorEvent e)
        {
            Debug.Log($"OnEnterDoorEvent triggered with ClientId: {e.ClientId}");
            
            // Send message to server
            using var writer = new FastBufferWriter(sizeof(ulong), Allocator.Temp);
            writer.WriteValueSafe(e.ClientId);
            NetworkManager.CustomMessagingManager.SendNamedMessage(ENTER_DOOR_MESSAGE, NetworkManager.ServerClientId, writer);
        }

        private void HandleEnterDoorMessage(ulong senderId, FastBufferReader reader)
        {
            if (!NetworkManager.IsServer) return;
            
            reader.ReadValueSafe(out ulong clientId);
            Debug.Log($"HandleEnterDoorMessage received on server for ClientId: {clientId}");
            
            var clientNetworkObj = NetworkUtils.GetNetworkObject(clientId);
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

        private void HandleEndGameMessage(ulong senderId, FastBufferReader reader)
        {
            Debug.Log("Game Ended");
        }

        public override void EndGameRpc()
        {
            SendEndGame();
        }

        private void SendEndGame()
        {
            // Create an empty writer since we don't need to send any data
            using var writer = new FastBufferWriter(0, Allocator.Temp);
            NetworkManager.CustomMessagingManager.SendNamedMessageToAll(END_GAME_MESSAGE, writer, NetworkDelivery.Reliable);
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
            return playerIndex < _playerColors.Length ? _playerColors[playerIndex] : Color.gray;
        }
    }
}