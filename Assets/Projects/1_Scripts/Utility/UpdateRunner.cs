using System;
using System.Collections.Generic;
using UnityEngine;

namespace PokaiLand.Infrastructure
{
    public class UpdateRunner : MonoBehaviour
    {
        private static UpdateRunner _instance;
        
        class SubscriberData
        {
            public float Period;
            public float NextCallTime;
            public float LastCallTime;
        }

        private readonly Queue<Action> _pendingHandlers = new Queue<Action>();
        private readonly HashSet<Action<float>> _subscribers = new HashSet<Action<float>>();
        private readonly Dictionary<Action<float>, SubscriberData> _subscriberData = new Dictionary<Action<float>, SubscriberData>();

        public static void Subscribe(Action<float> onUpdate, float updatePeriod)
        {
            EnsureInstance();
            _instance.InternalSubscribe(onUpdate, updatePeriod);
        }

        public static void Unsubscribe(Action<float> onUpdate)
        {
            EnsureInstance();
            _instance.InternalUnsubscribe(onUpdate);
        }

        private static void EnsureInstance()
        {
            if (_instance == null)
            {
                var go = new GameObject("UpdateRunner");
                _instance = go.AddComponent<UpdateRunner>();
                DontDestroyOnLoad(go);
            }
        }

        private void InternalSubscribe(Action<float> onUpdate, float updatePeriod)
        {
            if (onUpdate == null) return;
            if (onUpdate.Target == null)
            {
                Debug.LogError("Can't subscribe to a local function that can go out of scope");
                return;
            }
            if (onUpdate.Method.ToString().Contains("<"))
            {
                Debug.LogError("Can't subscribe with an anonymous function");
                return;
            }

            if (!_subscribers.Contains(onUpdate))
            {
                _pendingHandlers.Enqueue(() =>
                {
                    if (_subscribers.Add(onUpdate))
                    {
                        _subscriberData.Add(onUpdate, new SubscriberData 
                        { 
                            Period = updatePeriod, 
                            NextCallTime = 0, 
                            LastCallTime = Time.time 
                        });
                    }
                });
            }
        }

        private void InternalUnsubscribe(Action<float> onUpdate)
        {
            _pendingHandlers.Enqueue(() =>
            {
                _subscribers.Remove(onUpdate);
                _subscriberData.Remove(onUpdate);
            });
        }

        void Update()
        {
            while (_pendingHandlers.Count > 0)
            {
                _pendingHandlers.Dequeue()?.Invoke();
            }

            foreach (var subscriber in _subscribers)
            {
                var subscriberData = _subscriberData[subscriber];

                if (Time.time >= subscriberData.NextCallTime)
                {
                    subscriber.Invoke(Time.time - subscriberData.LastCallTime);
                    subscriberData.LastCallTime = Time.time;
                    subscriberData.NextCallTime = Time.time + subscriberData.Period;
                }
            }
        }

        public void OnDestroy()
        {
            _pendingHandlers.Clear();
            _subscribers.Clear();
            _subscriberData.Clear();
        }
    }
}