using System;
using System.Collections;
using System.Collections.Generic;
using static UGoap.Base.PropertyManager;

namespace UGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    public abstract class StateBase<T> : IEnumerable<KeyValuePair<PropertyKey, T>>
    {
        protected internal readonly SortedDictionary<PropertyKey, T> _values;
        
        public StateBase(StateBase<T> @base = null)
        {
            _values = @base == null ? new SortedDictionary<PropertyKey, T>()
                : new SortedDictionary<PropertyKey, T>(@base._values);
        }

        protected internal void AssertValidType(PropertyKey key, object value)
        {
            var type = PropertyManager.GetType(key);
            if (value.GetType() != type)
            {
                throw new ArgumentException("[GOAP] Type of value is not valid.");
            }
        }

        //Key Access
        public List<PropertyKey> GetPropertyKeys()
        {
            return new List<PropertyKey>(_values.Keys);
        }
        
        public bool Has(PropertyKey key)
        {
            return _values.ContainsKey(key);
        }
        
        public void Remove(PropertyKey key)
        {
            if(Has(key)) _values.Remove(key);
        }
        
        public bool IsEmpty()
        {
            return _values.Count == 0;
        }
        
        public IEnumerator<KeyValuePair<PropertyKey, T>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}