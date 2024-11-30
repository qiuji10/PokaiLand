using Cysharp.Threading.Tasks;

namespace PokaiLand.Gameplay.GameMode.V2
{
    public class DefaultGameMode : BaseGameMode, INetworkSystem<DefaultGameMode>
    {
        public override bool InitializedOnClientSide => false;
        public override bool CanStartGame() => true;
        
        public UniTask<DefaultGameMode> Init(params object[] args)
        {
            return UniTask.FromResult(this);
        }

        public override void ServerStartGame()
        {
        }

        public override void ServerStopGame()
        {
        }
    }
}