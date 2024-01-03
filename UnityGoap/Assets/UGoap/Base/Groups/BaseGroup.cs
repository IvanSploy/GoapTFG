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
    public abstract class BaseGroup<TKey, TValue> : IEnumerable<KeyValuePair<TKey,TValue>>
    {
        protected internal readonly SortedDictionary<TKey, TValue> Values;
        
        public BaseGroup(BaseGroup<TKey, TValue> effectGroup = null)
        {
            Values = effectGroup == null ? new SortedDictionary<TKey, TValue>()
                : new SortedDictionary<TKey, TValue>(effectGroup.Values);
        }

        //Key Access
        public List<TKey> GetKeys()
        {
            return new List<TKey>(Values.Keys);
        }
        
        public bool HasKey(TKey key)
        {
            return Values.ContainsKey(key);
        }
        
        public void Remove(TKey key)
        {
            if(HasKey(key)) Values.Remove(key);
        }
        
        public bool IsEmpty()
        {
            return Values.Count == 0;
        }
        
        //Overrides
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            BaseGroup<TKey, TValue> otherPg = (BaseGroup<TKey, TValue>)obj;
            
            if (CountRelevantKeys() != otherPg.CountRelevantKeys()) return false;
            foreach (var key in Values.Keys)
            {
                if (!otherPg.HasKey(key)) return false;
                if(!Values[key].Equals(otherPg.Values[key])) return false;
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
            foreach(var pair in Values)
            {
                //No se toman en cuenta las reglas desinformadas.
                if (!IsRelevantKey(pair.Key)) continue;
                
                hash = 18 * hash + (pair.Key.GetHashCode() ^ pair.Value.GetHashCode());
                hash %= int.MaxValue;
            }
            return hash;
        }

        #region DefaultValues
        private int CountRelevantKeys()
        {
            return Values.Keys.Count(IsRelevantKey);
        }

        private bool IsRelevantKey(TKey key)
        {
            return Values[key].GetHashCode() != GetDefaultValue(Values[key]).GetHashCode();
        }
        #endregion
        
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return Values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}