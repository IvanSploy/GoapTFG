﻿using System.Collections;
using System.Collections.Generic;

namespace UGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public abstract class GoapBase<TKey, TValue> : IEnumerable<KeyValuePair<TKey,TValue>>
    {
        protected internal readonly SortedDictionary<TKey, TValue> _values;
        
        public GoapBase(GoapBase<TKey, TValue> goapBase = null)
        {
            _values = goapBase == null ? new SortedDictionary<TKey, TValue>()
                : new SortedDictionary<TKey, TValue>(goapBase._values);
        }

        //Key Access
        public List<TKey> GetKeys()
        {
            return new List<TKey>(_values.Keys);
        }
        
        public bool HasKey(TKey key)
        {
            return _values.ContainsKey(key);
        }
        
        public void Remove(TKey key)
        {
            if(HasKey(key)) _values.Remove(key);
        }
        
        public bool IsEmpty()
        {
            return _values.Count == 0;
        }
        
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}