using System;
using System.Collections.Generic;
using UnityEngine;

namespace PokaiLand.Utilities
{
    public class CollisionHandler<T> where T : class
    {
        private readonly List<T> _nearbyObjects = new List<T>();
        private readonly Transform _playerTransform;
        private readonly Collider2D _collider2d;
        private readonly Func<T, Vector2> _getPosition;
        private readonly Func<T, bool> _isValid;
        private T _currentTarget;

        public CollisionHandler(Transform playerTransform, Collider2D collider2d, Func<T, Vector2> getPosition, Func<T, bool> isValid = null)
        {
            _playerTransform = playerTransform;
            _collider2d = collider2d;
            _getPosition = getPosition;
            _isValid = isValid ?? (_ => true);
        }

        public T CurrentTarget => _currentTarget;

        public void OnTriggerEnter(Collider2D col)
        {
            if (col.TryGetComponent(out T obj))
            {
                if (!_nearbyObjects.Contains(obj))
                {
                    _nearbyObjects.Add(obj);
                }

                ReevaluateTarget();
            }
        }

        public void OnTriggerExit(Collider2D col)
        {
            bool isRealExitFromSpecificObject = !Physics2D.IsTouching(_collider2d, col);
            
            if (isRealExitFromSpecificObject && col.TryGetComponent(out T obj) && _nearbyObjects.Contains(obj))
            {
                _nearbyObjects.Remove(obj);

                if (_currentTarget == obj)
                {
                    ReevaluateTarget();
                }
            }
        }

        private void ReevaluateTarget()
        {
            // Clean up invalid objects before finding closest
            _nearbyObjects.RemoveAll(obj => !_isValid(obj));
            
            _currentTarget = GameplayUtility.FindClosest(
                _nearbyObjects,
                _playerTransform.position,
                _getPosition,
                _isValid
            );
        }
    }
}