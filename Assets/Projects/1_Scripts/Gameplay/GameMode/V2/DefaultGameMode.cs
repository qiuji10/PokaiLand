using Cysharp.Threading.Tasks;
using PokaiLand.Player;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Gameplay.GameMode.V2
{
    public class DefaultGameMode : BaseGameMode, INetworkSystem<DefaultGameMode>
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
        
        public override bool InitializedOnClientSide => false;
        public override bool CanStartGame() => true;
        
        public UniTask<DefaultGameMode> Init(params object[] args)
        {
            return UniTask.FromResult(this);
        }

        protected override void ServerOnClientConnected(ulong clientId)
        {
            _playerNames.Add($"Player {clientId}");
            int index = NetworkManager.Singleton.ConnectedClientsList.Count - 1;
            UpdatePlayerColor(clientId, index);
        }

        public override void ServerStartGame()
        {
        }

        public override void ServerStopGame()
        {
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