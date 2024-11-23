namespace PokaiLand.Player
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public static class Utility
    {
        /// <summary>
        /// Finds the closest object to a given position from a collection based on a predicate filter.
        /// </summary>
        /// <typeparam name="T">The type of the objects in the collection.</typeparam>
        /// <param name="collection">The collection of objects to search through.</param>
        /// <param name="position">The reference position to calculate distances from.</param>
        /// <param name="positionSelector">A function to extract the position of each object.</param>
        /// <param name="predicate">A filter predicate to determine eligible objects.</param>
        /// <returns>The closest object to the given position, or default if none are eligible.</returns>
        public static T FindClosest<T>(
            IEnumerable<T> collection,
            Vector2 position,
            Func<T, Vector2> positionSelector,
            Func<T, bool> predicate = null)
        {
            T closest = default;
            float closestDistance = float.MaxValue;

            foreach (var obj in collection)
            {
                if (predicate != null && !predicate(obj)) continue;

                var objPosition = positionSelector(obj);
                float distance = Vector2.Distance(position, objPosition);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = obj;
                }
            }

            return closest;
        }
        
        /// <summary>
        /// Finds the closest object to a given position from a collection based on a predicate filter.
        /// </summary>
        /// <typeparam name="T">The type of the objects in the collection.</typeparam>
        /// <param name="collection">The collection of objects to search through.</param>
        /// <param name="position">The reference position to calculate distances from.</param>
        /// <param name="positionSelector">A function to extract the position of each object.</param>
        /// <param name="predicate">A filter predicate to determine eligible objects.</param>
        /// <returns>The closest object to the given position, or default if none are eligible.</returns>
        public static T FindClosest<T>(
            IEnumerable<T> collection,
            Vector3 position,
            Func<T, Vector3> positionSelector,
            Func<T, bool> predicate = null)
        {
            T closest = default;
            float closestDistance = float.MaxValue;

            foreach (var obj in collection)
            {
                if (predicate != null && !predicate(obj)) continue;

                var objPosition = positionSelector(obj);
                float distance = Vector3.Distance(position, objPosition);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closest = obj;
                }
            }

            return closest;
        }
    }

}