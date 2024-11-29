using UnityEngine;

namespace PokaiLand.Gameplay.Map
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Map Data", fileName = "MapDataSO")]
    public class MapDataSO : ScriptableObject
    {
        public MapData mapData;
    }
}