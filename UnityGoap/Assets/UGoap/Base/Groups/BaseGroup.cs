using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static UGoap.Base.BaseTypes;

namespace UGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public abstract class BaseGroup<TKey, TValue>
    {
        protected internal readonly SortedDictionary<TKey, GoapValue<TValue>> _values;
        
        public BaseGroup(BaseGroup<TKey, TValue> effectGroup = null)
        {
            _values = effectGroup == null ? new SortedDictionary<TKey, GoapValue<TValue>>()
                : new SortedDictionary<TKey, GoapValue<TValue>>(effectGroup._values);
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
        
        //Overrides
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            BaseGroup<TKey, TValue> otherPg = (BaseGroup<TKey, TValue>)obj;
            
            if (CountRelevantKeys() != otherPg.CountRelevantKeys()) return false;
            foreach (var key in _values.Keys)
            {
                if (!otherPg.HasKey(key)) return false;
                if(!_values[key].Value.Equals(otherPg._values[key].Value)) return false;
            }
            return true;
        }

        /// <summary>
        /// Evaluate hash code of the dictionary with sort order and xor exclusion.
        /// </summary>
        /// <returns>Hash Number</returns>
        public override int GetHashCode()
        {
            int hash = 18;
            foreach(KeyValuePair<TKey, GoapValue<TValue>> kvp in _values)
            {
                //No se toman en cuenta las reglas desinformadas.
                if (!IsRelevantKey(kvp.Key)) continue;
                
                hash = 18 * hash + (kvp.Key.GetHashCode() ^ kvp.Value.Value.GetHashCode());
                hash %= int.MaxValue;
            }
            return hash;
        }
        
        #region DefaultValues
        private int CountRelevantKeys()
        {
            return _values.Keys.Count(IsRelevantKey);
        }

        private bool IsRelevantKey(TKey key)
        {
            return _values[key].Value.GetHashCode() != GetDefaultValue(_values[key].Value).GetHashCode();
        }
        #endregion
        
        public class GroupEnumerator<T> : IEnumerator<KeyValuePair<TKey, T>>
        {
            private KeyValuePair<TKey, T>[] _data;
            private int _index = -1;

            public GroupEnumerator(TKey[] keys, T[] values)
            {
                _data = new KeyValuePair<TKey, T>[keys.Length];
                for (var i = 0; i < keys.Length; i++)
                {
                    var key = keys[i];
                    _data[i] = new KeyValuePair<TKey, T>(keys[i], values[i]);
                }
            }

            public KeyValuePair<TKey, T> Current
            {
                get
                {
                    if (_index >= 0 && _index < _data.Length)
                    {
                        return _data[_index];
                    }

                    throw new InvalidOperationException();
                }
            }

            object IEnumerator.Current => Current;
            
            public bool MoveNext()
            {
                _index++;
                return _index < _data.Length;
            }

            public void Reset()
            {
                _index = -1;
            }

            public void Dispose()
            { }
        }
    }
}