using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PokaiLand.Extensions;
using PokaiLand.Utility;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Gameplay.Map
{
    public class MapSystem : NetworkBehaviour, INetworkSystem<MapSystem>
    {
        [SerializeField] MapDataSO mapDataConfig;
        
        public MapDataSO MapDataConfig => mapDataConfig;
        public bool InitializedOnClientSide => false;
        
        public async UniTask<MapSystem> Init(params object[] args)
        {
            if (args == null || args.Length <= 0) return this;

            MapData mapData = new MapData();
            
            if (args[0] is string mapDataId)
            {
                mapDataConfig = await SystemUtility.FindAssetByName<MapDataSO>(mapDataId);
                mapData = mapDataConfig.mapData;
            }
            else if (args[0] is MapData data)
            {
                mapData = data;
            }

            LoadMapData(mapData);

            return this;
        }

        private async void LoadMapData(MapData mapData)
        {
            for (int i = 0; i < mapData.mapObjects.Length; i++)
            {
                var mapObjectData = mapData.mapObjects[i];
                var mapObject = await SystemUtility.CreateFromAsset<GameObject>(mapObjectData.labels);
                
                mapObject.SpawnOnNetwork();
                MapObjectProcessor.PostProcessMapObjectCreation(mapObject, mapObjectData);
            }
        }
        
        public void SaveMapData()
        {
            IMapObject[] worldObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<IMapObject>()
                .ToArray();

            List<MapObjectData> mapObjects = new List<MapObjectData>();
            
            for (int i = 0; i < worldObjects.Length; i++)
            {
                mapObjects.Add(CreateMapObject(worldObjects[i]));
            }

            MapData mapData = new MapData
            {
                mapObjects = mapObjects.ToArray()
            };
            
            mapDataConfig.mapData = mapData;
        }

        private MapObjectData CreateMapObject(IMapObject obj) => obj switch
        {
            ResetZone zone => new ResetZoneMapObjectData(zone.Labels, new Transform2D(zone.transform), zone.ResetPoint),
            _ => new MapObjectData(obj.Labels, new Transform2D(obj.gameObject.transform))
        };
    }
}
