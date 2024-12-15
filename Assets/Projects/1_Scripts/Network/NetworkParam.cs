using Unity.Netcode;

namespace PokaiLand.Network
{
    public class NetworkParam
    {
        public static BaseRpcTarget ToClients(RpcTarget target, params ulong[] clientIds)
        {
            return target.Group(clientIds, RpcTargetUse.Temp);
        }
    }
}