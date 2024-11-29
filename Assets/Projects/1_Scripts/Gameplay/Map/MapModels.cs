using System;
using PokaiLand.Enum;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Gameplay.Map
{
    [Serializable]
    public struct MapData : INetworkSerializable
    {
        public MapObject[] mapObjects;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref mapObjects);
        }
    }

    [Serializable]
    public struct MapObject : INetworkSerializable
    {
        public EAddressableLabels labels;
        public Transform2D transform;

        public MapObject(EAddressableLabels labels, Transform2D transform)
        {
            this.labels = labels;
            this.transform = transform;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref labels);
            serializer.SerializeValue(ref transform);
        }
    }
    
    [Serializable]
    public struct Transform2D : INetworkSerializable
    {
        public Vector2 position;
        public Vector2 size;
        public float rotation;

        public Transform2D(Transform transform)
        {
            position = transform.position;
            size = transform.localScale;
            rotation = transform.rotation.eulerAngles.z;
        }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref position);
            serializer.SerializeValue(ref size);
            serializer.SerializeValue(ref rotation);
        }
    }
}