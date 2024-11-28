using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using PokaiLand.Enum;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.GameMode
{
    public class GameModeManager : NetworkBehaviour, ISystem<GameModeManager>
    {
        [SerializeField] private EGameMode gameModeType;
        public BaseGameMode GameMode { get; private set; }

        /// <summary>
        /// Init GameMode
        /// </summary>
        /// <param name="args">NetworkBehaviour, CameraSystem</param>
        public GameModeManager Init(params object[] args)
        {
            var argsWithThis = new object[] { this }.Concat(args).ToArray();
            var type = GameModeConfig.Binding[gameModeType];
            GameMode = Activator.CreateInstance(type, argsWithThis) as BaseGameMode;
            return this;
        }

        public override void OnNetworkSpawn()
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

            if (GameMode == null)
                Init(CameraSystem.Instance);
            
            GameMode.OnNetworkStart();
        }
        
        public override void OnNetworkDespawn()
        {
            GameMode.OnNetworkStop();
            
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }

        private void OnClientConnected(ulong clientId)
        {
            if (IsServer)
            {
                GameMode.AddClientToIdList(clientId);
                GameMode.OnClientConnected(clientId);
            }
        }

        private void OnClientDisconnected(ulong clientId)
        {
            if (IsServer)
            {
                GameMode.RemoveClientFromIdList(clientId);
                GameMode.OnClientDisconnected(clientId);
            }
        }
    }
}
