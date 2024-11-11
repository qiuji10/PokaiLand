using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.GameMode
{
    using Player;

    public class GameManager : NetworkBehaviour
    {
        private NetworkManager Manager => NetworkManager.Singleton;
        
        public override void OnNetworkSpawn()
        {
            // _currentGameMode = GetGameMode();
            // _currentGameMode.StartGameMode();
            NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
        }
        
        public override void OnNetworkDespawn()
        {
            // _currentGameMode.EndGameMode();
            NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;
        }

        private void OnConnectionEvent(NetworkManager manager, ConnectionEventData e)
        {
            if (e.EventType == ConnectionEvent.ClientConnected)
            {
                if (IsServer)
                    UpdatePlayerColor(e.ClientId);
                else
                    UpdateNewPlayerColorClientRpc(e.ClientId);
            }
        }

        [ClientRpc]
        private void UpdateNewPlayerColorClientRpc(ulong clientId)
        {
            UpdatePlayerColor(clientId);
        }

        private void UpdatePlayerColor(ulong clientId)
        {
            var clientNetworkObject = Manager.SpawnManager.GetPlayerNetworkObject(clientId);
            var playerDeco = clientNetworkObject.GetComponent<PlayerDecorator>();
            playerDeco.playerColor.Value = GetPlayerColor(clientId);
        }
        
        private Color GetPlayerColor(ulong clientId)
        {
            int index = GetPlayerIndex() % Manager.ConnectedClientsList.Count;

            return index switch
            {
                0 => Color.red,
                1 => Color.green,
                2 => Color.blue,
                3 => Color.white,
                _ => Color.black
            };

            int GetPlayerIndex()
            {
                for (int i = 0; i < Manager.ConnectedClientsList.Count; i++)
                {
                    if (Manager.ConnectedClientsList[i].ClientId == clientId)
                        return i;
                }

                return 0;
            }
        }
    }
}