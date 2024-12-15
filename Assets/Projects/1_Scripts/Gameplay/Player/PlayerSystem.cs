using Cysharp.Threading.Tasks;
using PokaiLand.Enum;
using PokaiLand.Network;
using PokaiLand.Utility;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Player
{
    public class PlayerSystem : NetworkBehaviour, INetworkSystem<PlayerSystem>
    {
        private readonly RpcHandler _initPlayerRpcHandler = new RpcHandler();
        public bool InitializedOnClientSide => true;
        public async UniTask<PlayerSystem> Init(params object[] args)
        {
            await _initPlayerRpcHandler.Run(() => RequestServerSpawnPlayerRpc());
            return this;
        }

        [Rpc(SendTo.Server)]
        private void RequestServerSpawnPlayerRpc(RpcParams serverRpcParams = default)
        {
            ulong clientId = serverRpcParams.Receive.SenderClientId;
            SpawnPlayerAsync(clientId);
        }
        
        private async void SpawnPlayerAsync(ulong clientId)
        {
            GameObject player = await SystemUtility.CreateFromAsset<GameObject>(EAddressableLabels.Map | EAddressableLabels.Player);
            
            Debug.Log($"set player owner to {clientId}");
            
            var networkObject = player.GetComponent<NetworkObject>();
            networkObject.SpawnAsPlayerObject(clientId);
            
            ReturnClientSpawnPlayerDoneRpc(NetworkParam.ToClients(RpcTarget, clientId));
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void ReturnClientSpawnPlayerDoneRpc(RpcParams clientRpcParams = default)
        {
            _initPlayerRpcHandler.SetCompleted();
        }
    }
}