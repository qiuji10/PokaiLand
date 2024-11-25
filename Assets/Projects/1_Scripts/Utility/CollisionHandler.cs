using System;
using System.Collections.Generic;
using UnityEngine;

namespace PokaiLand.Utilities
{
    public class CollisionHandler<T> where T : class
    {
        private readonly List<T> _nearbyObjects = new List<T>();
        private readonly Transform _playerTransform;
        private readonly Func<T, Vector2> _getPosition;
        private readonly Func<T, bool> _isValid;
        private T _currentTarget;

        public CollisionHandler(Transform playerTransform, Func<T, Vector2> getPosition, Func<T, bool> isValid = null)
        {
            _playerTransform = playerTransform;
            _getPosition = getPosition;
            _isValid = isValid ?? (_ => true);
        }

        // Change to property with getter that always validates current target
        public T CurrentTarget 
        {
            get
            {
                // If current target is no longer valid, force a reevaluation
                if (_currentTarget != null)// && !_isValid(_currentTarget))
                {
                    ReevaluateTarget();
                }
                return _currentTarget;
            }
        }

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
            if (col.TryGetComponent(out T obj) && _nearbyObjects.Contains(obj))
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