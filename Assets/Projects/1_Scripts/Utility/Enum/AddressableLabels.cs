using System;

namespace PokaiLand.Enum
{
    [Flags]
    public enum EAddressableLabels
    {
        Camera = 1 << 0,
        Default = 1 << 1,
        GameMode = 1 << 2,
        Network = 1 << 3,
        System = 1 << 4,
    }
}
