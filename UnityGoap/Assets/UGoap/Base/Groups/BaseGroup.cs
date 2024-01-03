using System.Collections;
using System.Collections.Generic;

namespace UGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public abstract class BaseGroup<TKey, TValue> : IEnumerable<KeyValuePair<TKey,TValue>>
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
        
        public bool IsEmpty()
        {
            return _values.Count == 0;
        }
        
        public virtual IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}