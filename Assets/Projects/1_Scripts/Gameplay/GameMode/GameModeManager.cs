using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using PokaiLand.Enum;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.GameMode
{
    public class GameModeManager : NetworkBehaviour, INetworkSystem<GameModeManager>
    {
        [SerializeField] private EGameMode gameModeType;
        public BaseGameMode GameMode { get; private set; }

        /// <summary>
        /// Init GameMode
        /// </summary>
        /// <param name="args">NetworkBehaviour, CameraSystem</param>
        public UniTask<GameModeManager> Init(params object[] args)
        {
            var argsWithThis = new object[] { this }.Concat(args).ToArray();
            var type = GameModeConfig.Binding[gameModeType];
            GameMode = Activator.CreateInstance(type, argsWithThis) as BaseGameMode;

            if (GameMode == null)
                throw new NullReferenceException($"GameMode can't find matchable type {type}");
         
            
            
            return UniTask.FromResult(this);
        }
    }
}
