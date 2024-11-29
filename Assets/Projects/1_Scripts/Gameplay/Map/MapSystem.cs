using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using PokaiLand;
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
                var mapObject = await SystemUtility.CreateFromAsset<GameObject>(mapData.mapObjects[i].labels);
                mapObject.transform.Set(mapData.mapObjects[i].transform);
                mapObject.SpawnOnNetwork();
            }

            return this;
        }

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                SaveMapData();
            }
        }

        private void SaveMapData()
        {
            IInteractable[] interactables = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<IInteractable>()
                .ToArray();

            List<MapObject> mapObjects = new List<MapObject>();
            
            for (int i = 0; i < interactables.Length; i++)
            {
                mapObjects.Add(new MapObject
                {
                    labels = interactables[i].Labels,
                    transform = new Transform2D(interactables[i].gameObject.transform)
                });
            }

            MapData mapData = new MapData
            {
                mapObjects = mapObjects.ToArray()
            };
        }
    }
}
