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
        
            if (GUILayout.Button("Save Map Data", GUILayout.Height(30)))
            {
                mapSystem.SaveMapData();
            }
        }
    }
}