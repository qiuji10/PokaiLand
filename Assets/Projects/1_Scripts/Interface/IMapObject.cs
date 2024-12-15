using PokaiLand.Enum;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand
{
    public interface IMapObject
    {
        GameObject gameObject { get; }
        NetworkObject NetworkObject { get; }
        EAddressableLabels Labels { get; }
    }
}