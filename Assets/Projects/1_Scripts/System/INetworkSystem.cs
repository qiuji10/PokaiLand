using Cysharp.Threading.Tasks;
using Unity.Netcode;

namespace PokaiLand
{
    public interface ISystem<T>
    {
        UniTask<T> Init(params object[] args);
    }
    
    public interface INetworkSystem<T> : ISystem<T>
    {
        public NetworkObject NetworkObject { get; }
    }

    public interface ISystemBase
    {
        void Init(params object[] args);
    }
}