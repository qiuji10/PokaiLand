using Unity.Netcode;
using UnityEngine;

namespace PokaiLand.Utility
{
    public class SingletonNetworkBehaviour<T> : NetworkBehaviour where T : NetworkBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                _instance ??= FindFirstObjectByType<T>();
                
                if (_instance == null)
                    Debug.LogWarning($"[{typeof(T).Name}] Instance not found in scene.");
                    
                return _instance;
            }
            private set => _instance = value;
        }

        private bool PreventDestroyOnLoad => true;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
            }
            else
            {
                Debug.LogWarning($"[{typeof(T).Name}] Multiple instances detected. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            if (PreventDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_instance == this)
                _instance = null;
        }
    }
}