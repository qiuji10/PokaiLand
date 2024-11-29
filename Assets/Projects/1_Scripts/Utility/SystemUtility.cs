using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PokaiLand.Enum;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
        
        public static async UniTask<T> FindAsset<T>(EAddressableLabels labels)
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
    }
}