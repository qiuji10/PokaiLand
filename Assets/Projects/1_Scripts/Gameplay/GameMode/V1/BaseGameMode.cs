using System;
using System.Collections.Generic;
using System.Reflection;
using PokaiLand.Enum;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.GameMode.V1
{
    public abstract class BaseGameMode
    {
        protected readonly NetworkBehaviour NetworkBehaviour;
        protected readonly CameraNetworkSystem CameraNetworkSystem;
        protected bool NetworkInitialized => NetworkBehaviour != null && (NetworkBehaviour.IsServer || NetworkBehaviour.IsClient);
        protected static NetworkManager NetworkManager => NetworkManager.Singleton;

        #region Constructor Deconstructor
        protected BaseGameMode(params object[] args)
        {
            NetworkBehaviour = FindArgs<NetworkBehaviour>(args);
            CameraNetworkSystem = CameraNetworkSystem.Instance;
            InitNetworkVariables(NetworkBehaviour);
        }

        ~BaseGameMode()
        {
            OnDeconstructGameMode();
        }

        protected virtual void OnDeconstructGameMode()
        {
            DisposeNetworkVariables();
        }
        
        private T FindArgs<T>(object[] args)
        {
            var type = typeof(T);
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] is T)
                    return (T)args[i];
            }

            throw new NullReferenceException($"GameMode: Can't find args of type {type}");
        }
        #endregion

        #region Network Variables Reflection
        private void InitNetworkVariables(NetworkBehaviour networkBehaviour)
        {
            // Get all fields in this instance, including private and protected ones
            var fields = GetType().GetFields(BindingFlags.Instance | 
                                             BindingFlags.Public | 
                                             BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                // Check if the field is a NetworkVariable or derived from it
                if (IsNetworkVariableType(field.FieldType))
                {
                    var networkVar = field.GetValue(this);
                    // Call Initialize method using reflection
                    var initializeMethod = networkVar.GetType().GetMethod("Initialize");
                    initializeMethod?.Invoke(networkVar, new object[] { networkBehaviour });
                }
            }
        }
        
        private void DisposeNetworkVariables()
        {
            var fields = GetType().GetFields(BindingFlags.Instance | 
                                             BindingFlags.Public | 
                                             BindingFlags.NonPublic);

            foreach (var field in fields)
            {
                if (IsNetworkVariableType(field.FieldType))
                {
                    var networkVar = field.GetValue(this);
                    // Call Dispose method using reflection
                    var disposeMethod = networkVar.GetType().GetMethod("Dispose");
                    disposeMethod?.Invoke(networkVar, null);
                }
            }
        }
        
        private bool IsNetworkVariableType(Type type)
        {
            // Check if it's a NetworkVariable or NetworkList
            if (type.IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();
                return genericTypeDef == typeof(NetworkVariable<>) ||
                       genericTypeDef == typeof(NetworkList<>);
            }
            return false;
        }
        #endregion

        public abstract ECameraType CameraType { get; }

        public virtual void OnServerInit() { }
        public virtual void OnClientInit() { }
        public virtual void StartGame() { }
        public virtual void EndGame() { }
    }
}