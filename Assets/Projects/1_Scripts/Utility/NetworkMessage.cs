using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Network
{
    public static class NetworkMessage
    {
        public static void Register<T>(string name, Action<ulong, T> callback) where T : unmanaged, INetworkSerializable
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(name, (senderClientId, reader) =>
            {
                try
                {
                    reader.ReadNetworkSerializable(out T message);
                    callback.Invoke(senderClientId, message);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing message '{name}': {ex.Message}");
                }
            });
        }
         
        public static void Register(string name, Action<ulong> callback)
        {
            NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler(name, (senderClientId, reader) =>
            {
                try
                {
                    callback.Invoke(senderClientId);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error processing message '{name}': {ex.Message}");
                }
            });
        }
        
        public static void Unregister(string name)
        {
            NetworkManager.Singleton.CustomMessagingManager.UnregisterNamedMessageHandler(name);
        }

        public static void Send(string name, ulong toClient)
        {
            using var writer = new FastBufferWriter(0, Allocator.Temp);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(name, toClient, writer);
        }
        
        public static void Send<T>(string name, ulong toClient, T message) where T : unmanaged, INetworkSerializable
        {
            using var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize<T>(), Allocator.Temp);
            writer.WriteNetworkSerializable(message);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(name, toClient, writer);
        }
        
        public static void SendToServer(string name)
        {
            using var writer = new FastBufferWriter(0, Allocator.Temp);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(name, NetworkManager.ServerClientId, writer);
        }
        
        public static void SendToServer<T>(string name, T message) where T : unmanaged, INetworkSerializable
        {
            using var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize<T>(), Allocator.Temp);
            writer.WriteNetworkSerializable(message);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage(name, NetworkManager.ServerClientId, writer);
        }
        
        public static void SendToAll(string name)
        {
            using var writer = new FastBufferWriter(0, Allocator.Temp);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll(name, writer);
        }
        
        public static void SendToAll<T>(string name, T message) where T : unmanaged, INetworkSerializable
        {
            using var writer = new FastBufferWriter(FastBufferWriter.GetWriteSize<T>(), Allocator.Temp);
            writer.WriteNetworkSerializable(message);
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll(name, writer);
        }
    }
}