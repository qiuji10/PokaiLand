using System;
using UnityEngine;

namespace PokaiLand.Utility
{
    public class SingletonMonobehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                _instance ??= FindFirstObjectByType<T>();

                return _instance;
            }
        
            set => _instance = value;
        }

        protected virtual bool PreventDestroyOnLoad => true;

        private void Awake()
        {
            if (_instance == null)
                _instance = this as T;
            else
                Destroy(gameObject);
        
            if (PreventDestroyOnLoad)
                DontDestroyOnLoad(this);
        }

        private void OnDestroy()
        {
            _instance = null;
        }
    }

    public class Singleton<T> where T : new()
    {
        private static readonly Lazy<T> Lazy =  new(() => new T());
    
        public static T Instance => Lazy.Value;
    }
}