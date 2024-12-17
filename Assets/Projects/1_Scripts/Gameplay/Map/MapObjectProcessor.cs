using UnityEngine;
using PokaiLand.Extensions;

namespace PokaiLand.Gameplay.Map
{
    public static class MapObjectProcessor
    {
        public static void PostProcessMapObjectCreation(GameObject mapObject, MapObjectData mapObjectData)
        {
            mapObject.transform.Set(mapObjectData.transform);
                
            switch (mapObjectData)
            {
                case ResetZoneMapObjectData resetZoneData: HandleCustomData(mapObject, resetZoneData); break;
                // Add more cases here for other specific MapObject types
            }
        }
        
        private static void HandleCustomData<T>(GameObject mapObject, T data) where T : MapObjectData
        {
            if (mapObject.TryGetComponent<IPostProcessMapObjectData<T>>(out var processor))
            {
                processor.HandlePostProcessMapObjectData(data);
            }
        }
    }
}