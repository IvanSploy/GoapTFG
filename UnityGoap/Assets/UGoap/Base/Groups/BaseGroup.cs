using System;
using System.Collections;
using System.Collections.Generic;

namespace UGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public abstract class BaseGroup<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        protected internal readonly SortedDictionary<TKey, TValue> _values;
        
        public BaseGroup(BaseGroup<TKey, TValue> effectGroup = null)
        {
            _values = effectGroup == null ? new SortedDictionary<TKey, TValue>()
                : new SortedDictionary<TKey, TValue>(effectGroup._values);
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
        
        //Value access
        protected virtual void Set(TKey key, TValue value)
        {
            _values[key] = value;
        }
        
        public void Set(BaseGroup<TKey, TValue> otherPg)
        {
            foreach (var pair in otherPg)
            {
                Set(pair.Key, pair.Value);
            }
        }
        
        public TValue Get(TKey key)
        {
            return _values[key];
        }
        
        
        public TValue this[TKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }
        
        public bool IsEmpty()
        {
            return _values.Count == 0;
        }
        
        //Operators
        public static StateGroup<TKey, TValue> operator +(StateGroup<TKey, TValue> a, BaseGroup<TKey, TValue> b)
        {
            if (b == null) return a;
            
            var propertyGroup = new StateGroup<TKey, TValue>(a);
            
            foreach (var pair in b)
            {
                propertyGroup.Set(pair.Key, pair.Value);
            }
            return propertyGroup;
        }
        
        public static StateGroup<TKey, TValue> operator -(StateGroup<TKey, TValue> a, BaseGroup<TKey, TValue> b)
        {
            if (b == null) return a;
            
            var propertyGroup = new StateGroup<TKey, TValue>(a);
            
            foreach (var pair in b)
            {
                if(a.HasKey(pair.Key)) propertyGroup.Remove(pair.Key);
            }
            return propertyGroup;
        }
        
        //Enumerable
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