using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PokaiLand.Events;
using PokaiLand.Player;
using PokaiLand.Utility;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Gameplay.GameMode.V2
{
    public class DefaultGameMode : BaseGameMode, INetworkSystem<DefaultGameMode>, IDisposable
    {
        private readonly NetworkList<FixedString32Bytes> _playerNames = new();
        private readonly List<Color> _registeredColors = new();
        private int _playerCompleteCount;
        
        private readonly Color[] _colorIndexes = new[]
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
        };
        
        public override bool InitializedOnClientSide => false;
        public override bool CanStartGame() => true;


        public UniTask<DefaultGameMode> Init(params object[] args)
        {
            ServerStartGame();
            return UniTask.FromResult(this);
        }
        
        public override void ServerStartGame()
        {
            
        }

        public override void ServerStopGame()
        {
            Debug.Log("game stop");
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            
            if (IsServer)
                EventBus.Register<ServerEnterDoorEvent>(OnEnterDoorEvent);
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            
            if (IsServer)
                EventBus.Unregister<ServerEnterDoorEvent>(OnEnterDoorEvent);
        }
        
        [Rpc(SendTo.Server, DeferLocal = true)]
        protected override void ServerOnClientGameModeSpawnRpc(ulong clientId)
        {
            base.ServerOnClientGameModeSpawnRpc(clientId);
            
            _playerNames.Add($"Player {clientId}");
            UpdatePlayerColor(clientId, FindClientIndex(clientId));
        }
        
        private void OnEnterDoorEvent(ServerEnterDoorEvent e)
        {
            if (e.IsEnterDoor)
                _playerCompleteCount++;
            else
                _playerCompleteCount--;
            
            if (_playerCompleteCount >= _playerNames.Count)
            {
                ServerStopGame();
            }
        }

        private int FindClientIndex(ulong clientId)
        {
            for (int i = 0; i < NetworkManager.Singleton.ConnectedClientsList.Count; i++)
            {
                if (NetworkManager.Singleton.ConnectedClientsList[i].ClientId == clientId)
                {
                    return i;
                }
            }

            return -1;
        }

        private void UpdatePlayerColor(ulong clientId, int playerIndex)
        {
            var clientNetworkObject = NetworkManager.SpawnManager.GetPlayerNetworkObject(clientId);
            var playerDeco = clientNetworkObject?.GetComponent<PlayerDecorator>();
            if (playerDeco != null)
            {
                playerDeco.playerName.Value = $"Player {playerIndex}";
                playerDeco.playerColor.Value = GetAvailableColor();
            }
        }

        private Color GetAvailableColor()
        {
            for (int i = 0; i < _colorIndexes.Length; i++)
            {
                if (!_registeredColors.Contains(_colorIndexes[i]))
                {
                    _registeredColors.Add(_colorIndexes[i]);
                    return _colorIndexes[i];
                }
            }
            
            return Color.grey;
        }
        
        private Color GetPlayerColor(int playerIndex)
        {
            return playerIndex < _colorIndexes.Length ? _colorIndexes[playerIndex] : Color.gray;
        }

        public void Dispose()
        {
            _playerNames?.Dispose();
        }
    }
}