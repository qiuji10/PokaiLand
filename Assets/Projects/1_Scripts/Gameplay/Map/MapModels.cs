using System;
using PokaiLand.Enum;
using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Gameplay.Map
{    
    [Serializable]
    public struct Transform2D : INetworkSerializable, IEquatable<Transform2D>
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

        public bool Equals(Transform2D other)
        {
            return position.Equals(other.position) && size.Equals(other.size) && rotation.Equals(other.rotation);
        }

        public override bool Equals(object obj)
        {
            return obj is Transform2D other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(position, size, rotation);
        }
    }
    
    [Serializable]
    public struct MapData : INetworkSerializable
    {
        [SerializeReference, SubclassSelector]
        public MapObjectData[] mapObjects;
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            // For class arrays, we need to handle null cases and initialization
            if (serializer.IsReader)
            {
                int length = 0;
                serializer.SerializeValue(ref length);
                mapObjects = new MapObjectData[length];
                
                for (int i = 0; i < length; i++)
                {
                    mapObjects[i] = new MapObjectData();
                    serializer.SerializeValue(ref mapObjects[i]);
                }
            }
            else
            {
                int length = mapObjects?.Length ?? 0;
                serializer.SerializeValue(ref length);
                
                for (int i = 0; i < length; i++)
                {
                    serializer.SerializeValue(ref mapObjects[i]);
                }
            }
        }
    }

    [Serializable]
    public class MapObjectData : INetworkSerializable
    {
        public EAddressableLabels labels;
        public Transform2D transform;

        // Default constructor needed for serialization
        public MapObjectData()
        {
            labels = default;
            transform = new Transform2D();
        }

        public MapObjectData(EAddressableLabels labels, Transform2D transform)
        {
            this.labels = labels;
            this.transform = transform;
        }

        public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref labels);
            serializer.SerializeValue(ref transform);
        }

        // Optional: Override equals for proper comparison
        public override bool Equals(object obj)
        {
            if (obj is MapObjectData other)
            {
                return labels == other.labels && transform.Equals(other.transform);
            }
            return false;
        }

        // Optional: Override GetHashCode when overriding Equals
        public override int GetHashCode()
        {
            return HashCode.Combine(labels, transform);
        }
    }
    
    [Serializable]
    public class ResetZoneMapObjectData : MapObjectData
    {
        public Vector2 spawnPoint;

        public ResetZoneMapObjectData() : base()
        {
            spawnPoint = Vector2.zero;
        }

        public ResetZoneMapObjectData(EAddressableLabels labels, Transform2D transform, Vector2 spawnPoint) : base(labels, transform)
        {
            this.spawnPoint = spawnPoint;
        }

        public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
        {
            base.NetworkSerialize(serializer);
            serializer.SerializeValue(ref spawnPoint);
        }

        public override bool Equals(object obj)
        {
            if (obj is ResetZoneMapObjectData other)
            {
                return base.Equals(obj) && spawnPoint == other.spawnPoint;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), spawnPoint);
        }
    }
}