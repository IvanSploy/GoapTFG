using System.Collections;
using System.Collections.Generic;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    /// <typeparam name="PropertyKey">Key type</typeparam>
    /// <typeparam name="object">Value type</typeparam>
    public abstract class GoapBase<T> : IEnumerable<KeyValuePair<PropertyKey, T>>
    {
        protected internal readonly SortedDictionary<PropertyKey, T> _values;
        
        public GoapBase(GoapBase<T> goapBase = null)
        {
            _values = goapBase == null ? new SortedDictionary<PropertyKey, T>()
                : new SortedDictionary<PropertyKey, T>(goapBase._values);
        }

        //Key Access
        public List<PropertyKey> GePropertyKeys()
        {
            return new List<PropertyKey>(_values.Keys);
        }
        
        public bool HasKey(PropertyKey key)
        {
            return _values.ContainsKey(key);
        }
        
        public void Remove(PropertyKey key)
        {
            if(HasKey(key)) _values.Remove(key);
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