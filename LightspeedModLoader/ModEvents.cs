using System;
using System.Collections.Generic;

namespace LightspeedModLoader
{
    public static class ModEvents
    {
        private static readonly Dictionary<string, Action<object>> _eventDictionary = new Dictionary<string, Action<object>>();

        public static void Subscribe(string eventName, Action<object> listener)
        {
            if (!_eventDictionary.ContainsKey(eventName))
                _eventDictionary[eventName] = delegate { };

            _eventDictionary[eventName] += listener;
        }

        public static void Unsubscribe(string eventName, Action<object> listener)
        {
            if (_eventDictionary.ContainsKey(eventName))
                _eventDictionary[eventName] -= listener;
        }

        public static void Publish(string eventName, object eventData = null)
        {
            if (_eventDictionary.ContainsKey(eventName))
                _eventDictionary[eventName].Invoke(eventData);
        }
    }
}
