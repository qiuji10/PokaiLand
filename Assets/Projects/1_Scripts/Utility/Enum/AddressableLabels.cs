using System;

namespace PokaiLand.Enum
{
    [Flags]
    public enum EAddressableLabels : uint
    {
        Block = 4096u, // 0x00001000
        Camera = 2147483648u, // 0x80000000
        Default = 65536u, // 0x00010000
        Door = 268435456u, // 0x10000000
        GameMode = 134217728u, // 0x08000000
        Interactive = 4u, // 0x00000004
        Key = 16u, // 0x00000010
        Map = 32768u, // 0x00008000
        Network = 131072u, // 0x00020000
        Player = 1024u, // 0x00000400
        ResetZone = 256u, // 0x00000100
        System = 33554432u, // 0x02000000
    }
}
