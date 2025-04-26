using System;
using System.Collections;
using System.Collections.Generic;
using static QGoap.Base.PropertyManager;

namespace QGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    public abstract class Group<T> : IEnumerable<KeyValuePair<PropertyManager.PropertyKey, T>>
    {
        protected internal readonly SortedDictionary<PropertyManager.PropertyKey, T> _values;
        
        public Group(Group<T> group = null)
        {
            _values = group == null ? new SortedDictionary<PropertyManager.PropertyKey, T>()
                : new SortedDictionary<PropertyManager.PropertyKey, T>(group._values);
        }

        protected void AssertValidType(PropertyManager.PropertyKey key, object value)
        {
            var type = PropertyManager.GetType(key);
            if (value.GetType() != type)
            {
                throw new ArgumentException("[GOAP] Type of value is not valid.");
            }
        }

        //Key Access
        public List<PropertyManager.PropertyKey> GetPropertyKeys()
        {
            return new List<PropertyManager.PropertyKey>(_values.Keys);
        }
        
        public bool Has(PropertyManager.PropertyKey key)
        {
            return _values.ContainsKey(key);
        }
        
        public void Remove(PropertyManager.PropertyKey key)
        {
            if(Has(key)) _values.Remove(key);
        }
        
        public bool IsEmpty()
        {
            return _values.Count == 0;
        }
        
        public IEnumerator<KeyValuePair<PropertyManager.PropertyKey, T>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}