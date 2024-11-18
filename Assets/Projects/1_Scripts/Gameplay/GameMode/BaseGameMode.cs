using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.GameMode
{
    public abstract class BaseGameMode
    {
        [SerializeField] protected List<ulong> ClientIds = new List<ulong>();
        protected static NetworkManager NetworkManager => NetworkManager.Singleton;
        public void AddClientToIdList(ulong clientId) => ClientIds.Add(clientId);
        public void RemoveClientFromIdList(ulong clientId) => ClientIds.Remove(clientId);
        public int GetPlayerCount() => ClientIds.Count;

        public virtual void OnClientConnected(ulong clientId)
        {
        }

        public virtual  void OnClientDisconnected(ulong clientId)
        {
        }
        
        public virtual void StartGame()
        {
        }

        public virtual void EndGame()
        {
        }
    }
}