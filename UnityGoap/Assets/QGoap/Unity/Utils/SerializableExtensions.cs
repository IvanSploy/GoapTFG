using System;
using System.Collections.Generic;
using UnityEngine;

namespace QGoap.Unity.Utils
{
    [Serializable]
    public struct SerializablePair<TKey, TValue>
    {
        [SerializeReference] public TKey Key;
        [SerializeReference] public TValue Value;

        public SerializablePair(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
    
    [Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        [SerializeField] private List<SerializablePair<TKey, TValue>> _entries;
        
        public void FromDictionary(Dictionary<TKey, TValue> dictionary)
        {
            _entries.Clear();
            foreach (var kvp in dictionary)
            {
                _entries.Add(new(kvp.Key, kvp.Value));
            }
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            var dictionary = new Dictionary<TKey, TValue>();
            foreach (var entry in _entries)
            {
                if(dictionary.ContainsKey(entry.Key)) continue;
                dictionary[entry.Key] = entry.Value;
            }
            return dictionary;
        }
    }
}