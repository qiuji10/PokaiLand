using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.GameMode
{
    public abstract class BaseGameMode
    {
        [SerializeField] protected NetworkList<ulong> ClientIds = new NetworkList<ulong>();
        protected static NetworkManager NetworkManager => NetworkManager.Singleton;

        #region Constructor Deconstructor
        protected BaseGameMode(NetworkBehaviour networkBehaviour)
        {
            InitNetworkVariables(networkBehaviour);
        }
        
        ~BaseGameMode()
        {
            DisposeNetworkVariables();
        }
        #endregion

        #region Network Variables
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
        
        #region Client Ids
        public void AddClientToIdList(ulong clientId) => ClientIds.Add(clientId);
        public void RemoveClientFromIdList(ulong clientId) => ClientIds.Remove(clientId);
        public int GetPlayerCount() => ClientIds.Count;
        #endregion
        
        public virtual void StartGame() { }
        public virtual void EndGame() { }
        public virtual void OnClientConnected(ulong clientId) { }
        public virtual void OnClientDisconnected(ulong clientId) { }
    }
}