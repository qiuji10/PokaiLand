using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace PokaiLand.Utility
{
    public static class SystemUtility
    {
        public static async UniTask<T> FindAsset<T>(params string[] labels)
        {
            var locationsHandle = Addressables.LoadResourceLocationsAsync(labels, Addressables.MergeMode.Intersection);
            var locations = await locationsHandle.Task;

            if (locations == null || locations.Count == 0)
                throw new Exception($"No assets found matching labels: {string.Join(", ", labels)}");

            var component = await Addressables.LoadAssetAsync<T>(locations[0]);
            return component;
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
    }
}