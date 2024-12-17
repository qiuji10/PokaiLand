using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PokaiLand.Enum;
using UnityEngine;
using UnityEngine.AddressableAssets;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.AddressableAssets;
#endif

namespace PokaiLand.Utility
{
    public static class SystemUtility
    {
        private static readonly IEnumerable<EAddressableLabels> AddressableLabels = System.Enum.GetValues(typeof(EAddressableLabels)).Cast<EAddressableLabels>();
        
        public static async UniTask<T> FindAssetByName<T>(string name)
        {
            var operationHandle = Addressables.LoadAssetAsync<T>(name);
            return await operationHandle.Task;
        }
        
        public static async UniTask<T> FindAssetByLabels<T>(params string[] labels)
        {
            var locationsHandle = Addressables.LoadResourceLocationsAsync(labels, Addressables.MergeMode.Intersection);
            var locations = await locationsHandle.Task;

            if (locations == null || locations.Count == 0)
                throw new Exception($"No assets found matching labels: {string.Join(", ", labels)}");

            var component = await Addressables.LoadAssetAsync<T>(locations[0]);
            return component;
        }
        
        public static async UniTask<T> FindAssetByLabels<T>(EAddressableLabels labels)
        {
            var array = AddressableLabels
                .Where(l => labels.HasFlag(l))
                .Select(l => l.ToString())
                .ToArray();

            return await FindAssetByLabels<T>(array);
        }
        
        public static async UniTask<T> CreateFromAsset<T>(params string[] labels)
        {
            var locationsHandle = Addressables.LoadResourceLocationsAsync(labels, Addressables.MergeMode.Intersection);
            var locations = await locationsHandle.Task;

            if (locations == null || locations.Count == 0)
                throw new Exception($"No assets found matching labels: {string.Join(", ", labels)}");

            var instantiatedObject = await Addressables.InstantiateAsync(locations[0]);

            if (typeof(T) == typeof(GameObject) || typeof(T) == typeof(ScriptableObject))
                return (T)(object)instantiatedObject;

            if (instantiatedObject != null && instantiatedObject.TryGetComponent<T>(out var component))
                return component;

            throw new Exception($"Failed to instantiate or find component of type {typeof(T)} for labels: {string.Join(", ", labels)}");
        }
        
        public static async UniTask<T> CreateFromAsset<T>(EAddressableLabels labels)
        {
            var array = AddressableLabels
                .Where(l => labels.HasFlag(l))
                .Select(l => l.ToString())
                .ToArray();
            
            return await CreateFromAsset<T>(array);
        }
        
        #if UNITY_EDITOR
        public static GameObject EditorFindPrefabByLabels(EAddressableLabels labels)
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (!settings)
                throw new InvalidOperationException("Addressable Asset Settings not found!");

            // Convert enum flags to string array
            var labelStrings = System.Enum.GetValues(typeof(EAddressableLabels))
                .Cast<EAddressableLabels>()
                .Where(l => labels.HasFlag(l))
                .Select(l => l.ToString())
                .ToArray();

            if (labelStrings.Length == 0)
                throw new ArgumentException("No labels selected!", nameof(labels));

            // Find entries with ALL specified labels
            var entries = settings.groups
                .SelectMany(g => g.entries)
                .Where(e => labelStrings.All(label => e.labels.Contains(label)))
                .ToList();

            if (entries.Count == 0)
                throw new InvalidOperationException($"No prefab found with labels: {string.Join(", ", labelStrings)}");

            if (entries.Count > 1)
                throw new InvalidOperationException($"Multiple prefabs found with labels: {string.Join(", ", labelStrings)}");

            var entry = entries[0];
            string assetPath = AssetDatabase.GUIDToAssetPath(entry.guid);
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
    
            if (!asset)
                throw new InvalidOperationException($"Failed to load prefab at path: {assetPath}");

            return asset;
        }
        #endif
    }
}