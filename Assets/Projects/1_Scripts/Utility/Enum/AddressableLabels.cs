using System;

namespace PokaiLand.Enum
{
    [Flags]
    public enum EAddressableLabels
    {
        Camera = 1 << 0,
        Default = 1 << 1,
        Door = 1 << 2,
        GameMode = 1 << 3,
        Interactive = 1 << 4,
        Key = 1 << 5,
        Map = 1 << 6,
        Network = 1 << 7,
        System = 1 << 8,
    }
}
