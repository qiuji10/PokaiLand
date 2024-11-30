using System;
using System.Collections.Generic;
using PokaiLand.Enum;

namespace PokaiLand.GameMode.V1
{
    public class GameModeConfig
    {
        public static Dictionary<EGameMode, Type> Binding = new()
        {
            { EGameMode.Basic , typeof(DefaultGameMode) }
        };
    }
}