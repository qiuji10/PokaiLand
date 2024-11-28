using Cinemachine;
using Cysharp.Threading.Tasks;
using PokaiLand.Infrastructure;
using PokaiLand;
using PokaiLand.Enum;
using PokaiLand.Utility;
using UnityEngine;

public class CameraNetworkSystem : SingletonMonobehaviour<CameraNetworkSystem>, ISystem<CameraNetworkSystem>
{
    public async UniTask<CameraNetworkSystem> Init(params object[] args)
    {
        await SystemUtility.CreateFromAsset<GameObject>("Camera", ((ECameraType)args[0]).ToString());
        return this;
    }
}
