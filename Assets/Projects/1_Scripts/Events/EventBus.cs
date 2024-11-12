namespace PokaiLand.Events
{
    using System;
    using System.Collections.Generic;

    public static class EventBus
    {
        private static Dictionary<Type, List<Delegate>> _eventListeners = new();

        // Register an event listener for an event of type T
        public static void Register<T>(Action<T> listener)
        {
            if (!_eventListeners.TryGetValue(typeof(T), out List<Delegate> listeners))
            {
                listeners = new List<Delegate>();
                _eventListeners[typeof(T)] = listeners;
            }
            listeners.Add(listener);
        }

        // Unregister an event listener for an event of type T
        public static void Deregister<T>(Action<T> listener)
        {
            if (_eventListeners.TryGetValue(typeof(T), out List<Delegate> listeners))
            {
                listeners.Remove(listener);
            }
        }

        // Execute (or publish) an event of type T, with an optional parameter
        public static void Execute<T>(T data = default)
        {
            if (_eventListeners.TryGetValue(typeof(T), out List<Delegate> listeners))
            {
                foreach (var listener in listeners)
                {
                    ((Action<T>)listener)?.Invoke(data);
                }
            }
        }
    }

}