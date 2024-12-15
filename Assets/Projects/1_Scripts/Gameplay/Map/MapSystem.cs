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
        public bool InitializedOnClientSide => false;
        
        public async UniTask<MapSystem> Init(params object[] args)
        {
            if (args == null || args.Length <= 0) return this;

            MapData mapData = new MapData();
            
            if (args[0] is string mapDataId)
            {
                MapDataSO mapDataConfig = await SystemUtility.FindAssetByName<MapDataSO>(mapDataId);
                mapData = mapDataConfig.mapData;
            }
            else if (args[0] is MapData data)
            {
                mapData = data;
            }

            for (int i = 0; i < mapData.mapObjects.Length; i++)
            {
                var mapObjectData = mapData.mapObjects[i];
                var mapObject = await SystemUtility.CreateFromAsset<GameObject>(mapObjectData.labels);
                
                mapObject.SpawnOnNetwork();
                mapObject.transform.Set(mapObjectData.transform);

                switch (mapObjectData)
                {
                    case ResetZoneMapObjectData resetZone:
                        if (mapObject.TryGetComponent<ResetZone>(out var zoneComponent))
                        {
                            zoneComponent.DefineResetPosition(resetZone.spawnPoint);
                        }
                        break;
                    // Add more cases here for other specific MapObject types
                }


            }

            return this;
        }
        
        #if UNITY_EDITOR

        [SerializeField] MapDataSO mapDataSO;
        
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
            
            mapDataSO.mapData = mapData;
        }

        private MapObjectData CreateMapObject(IMapObject obj) => obj switch
        {
            ResetZone zone => new ResetZoneMapObjectData(zone.Labels, new Transform2D(zone.transform), zone.ResetPoint),
            _ => new MapObjectData(obj.Labels, new Transform2D(obj.gameObject.transform))
        };
        #endif
    }
}
