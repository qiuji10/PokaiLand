using PokaiLand.Gameplay.Map;
using UnityEngine;

namespace PokaiLand.Extensions
{
    public static class TransformExtensions
    {
        public static void Set(this Transform transform, Transform2D transform2D)
        {
            transform.position = transform2D.position;
            transform.localScale = transform2D.size;
            transform.eulerAngles = new Vector3(0, 0, transform2D.rotation);
        }
    }
}