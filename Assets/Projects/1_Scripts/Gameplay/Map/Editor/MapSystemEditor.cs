using System.Linq;
using PokaiLand.Extensions;
using PokaiLand.Utility;

namespace PokaiLand.Gameplay.Map.Editor
{
    using UnityEditor;
    using UnityEngine;

    [CustomEditor(typeof(MapSystem))]
    public class MapSystemEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            MapSystem mapSystem = (MapSystem)target;

            EditorGUILayout.Space(10);
        
            if (GUILayout.Button("Load Map Data", GUILayout.Height(30)))
            {
                EditorLoadMapData();
            }
            
            if (GUILayout.Button("Save Map Data", GUILayout.Height(30)))
            {
                mapSystem.SaveMapData();
            }
        }
        
        private void EditorLoadMapData()
        {
            var existingMapObjects = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .OfType<IMapObject>()
                .Select(mapObject => (mapObject as MonoBehaviour)?.gameObject)
                .Where(go => go);
    
            foreach (var mapObject in existingMapObjects)
            {
                DestroyImmediate(mapObject);
            }
            
            MapSystem system = target as MapSystem;
            System.Diagnostics.Debug.Assert(system, nameof(system) + " != null");
            MapData mapData = system.MapDataConfig.mapData;
            
            for (int i = 0; i < mapData.mapObjects.Length; i++)
            {
                var mapObjectData = mapData.mapObjects[i];
                var mapObjectPrefab = SystemUtility.EditorFindPrefabByLabels(mapObjectData.labels);
                GameObject mapObject = (GameObject)PrefabUtility.InstantiatePrefab(mapObjectPrefab);
                MapObjectProcessor.PostProcessMapObjectCreation(mapObject, mapObjectData);
            }
        }
    }
}