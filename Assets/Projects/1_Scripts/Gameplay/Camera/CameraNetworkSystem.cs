using Cinemachine;
using Cysharp.Threading.Tasks;
using PokaiLand.Infrastructure;
using PokaiLand;
using PokaiLand.Enum;
using PokaiLand.Utility;
using Unity.Netcode;
using UnityEngine;

public class CameraNetworkSystem : SingletonNetworkBehaviour<CameraNetworkSystem>, INetworkSystem<CameraNetworkSystem>
{
    public bool InitializedOnClientSide => true;
    
    public async UniTask<CameraNetworkSystem> Init(params object[] args)
    { 
        await SystemUtility.CreateFromAsset<GameObject>(EAddressableLabels.Camera.ToString(), ((ECameraType)args[0]).ToString());
        return this;
    }


}
