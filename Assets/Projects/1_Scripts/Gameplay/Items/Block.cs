using PokaiLand;
using PokaiLand.Enum;
using Unity.Netcode;
using UnityEngine;

public class Block : MonoBehaviour, IMapObject
{
    public NetworkObject NetworkObject => null;
    public EAddressableLabels Labels => EAddressableLabels.Map | EAddressableLabels.Block;
}
