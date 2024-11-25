namespace PokaiLand.Events
{
    using UnityEngine;
    
    [DefaultExecutionOrder(-9999)] // Ensure this runs very early
    public static class EventBusInitializer 
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            // Initialize EventBus
            EventBus.Initialize();
            
            // Register to handle game quit/destroy
            Application.quitting += OnApplicationQuit;
            
            // Optional: Handle domain reload in editor
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            #endif
        }

        private static void OnApplicationQuit()
        {
            EventBus.Dispose();
        }

        #if UNITY_EDITOR
        private static void OnPlayModeStateChanged(UnityEditor.PlayModeStateChange state)
        {
            if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            {
                EventBus.Dispose();
            }
        }
        #endif
    }
}

namespace PokaiLand.Events
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    public static class EventBus
    {
        private static Dictionary<Type, List<Delegate>> _eventListeners = new();
        private static bool _isDisposed = false;
        private static readonly object _lock = new object();
        private static HashSet<Type> _activeEventTypes = new();

        internal static void Initialize()
        {
            lock (_lock)
            {
                _isDisposed = false;
                _eventListeners = new Dictionary<Type, List<Delegate>>();
                _activeEventTypes = new HashSet<Type>();
                //Debug.Log("EventBus initialized");
            }
        }

        internal static void Dispose()
        {
            lock (_lock)
            {
                if (_isDisposed)
                {
                    return;
                }

                _isDisposed = true;

                // Log disposed listeners for debugging
                // foreach (var kvp in _eventListeners)
                // {
                //     if (kvp.Value.Count > 0)
                //     {
                //         Debug.Log($"Disposing EventBus - Clearing {kvp.Value.Count} listeners for {kvp.Key.Name}");
                //     }
                // }

                _activeEventTypes = new HashSet<Type>(_eventListeners.Keys);
                _eventListeners.Clear();
                //Debug.Log("EventBus disposed");
            }
        }

        public static void Register<T>(Action<T> listener)
        {
            if (_isDisposed)
            {
                Debug.LogWarning($"Attempted to register event listener after EventBus disposal: {typeof(T).Name}");
                return;
            }

            lock (_lock)
            {
                if (!_eventListeners.TryGetValue(typeof(T), out List<Delegate> listeners))
                {
                    listeners = new List<Delegate>();
                    _eventListeners[typeof(T)] = listeners;
                    _activeEventTypes.Add(typeof(T));
                }
                listeners.Add(listener);
            }
        }

        public static void Deregister<T>(Action<T> listener)
        {
            lock (_lock)
            {
                if (_eventListeners.TryGetValue(typeof(T), out List<Delegate> listeners))
                {
                    listeners.Remove(listener);
                    
                    if (listeners.Count == 0)
                    {
                        _eventListeners.Remove(typeof(T));
                        _activeEventTypes.Remove(typeof(T));
                    }
                }
            }
        }

        public static void Execute<T>(T data = default)
        {
            if (_isDisposed)
            {
                if (_activeEventTypes.Contains(typeof(T)))
                {
                    Debug.LogWarning($"Attempted to execute event after EventBus disposal: {typeof(T).Name}");
                }
                return;
            }

            List<Delegate> listenersCopy;
            lock (_lock)
            {
                if (!_eventListeners.TryGetValue(typeof(T), out List<Delegate> listeners))
                {
                    return;
                }
                listenersCopy = new List<Delegate>(listeners);
            }

            foreach (var listener in listenersCopy)
            {
                try
                {
                    ((Action<T>)listener)?.Invoke(data);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error executing event {typeof(T).Name}: {ex}");
                }
            }
        }

        public static bool HasListeners<T>()
        {
            lock (_lock)
            {
                return _eventListeners.TryGetValue(typeof(T), out var listeners) && listeners.Count > 0;
            }
        }

        public static int GetListenerCount<T>()
        {
            lock (_lock)
            {
                return _eventListeners.TryGetValue(typeof(T), out var listeners) ? listeners.Count : 0;
            }
        }
    }
}