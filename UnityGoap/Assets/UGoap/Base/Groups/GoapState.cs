using System;
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
    public class GoapState<TKey, TValue> : GoapBase<TKey, TValue>
    {
        public GoapState(GoapState<TKey, TValue> goapState = null) : base(goapState)
        { }
        
        //Value Access
        public void Set(TKey key, TValue value)
        {
            _values[key] = value;
        }
        
        public void Set(GoapState<TKey, TValue> otherPg)
        {
            foreach (var pair in otherPg)
            {   
                Set(pair.Key, pair.Value);
            }
        }
        
        public void Set(GoapEffects<TKey, TValue> goapEffects)
        {
            foreach (var pair in goapEffects)
            {   
                Set(pair.Key, pair.Value.Value);
            }
        }
        
        private TValue Get(TKey key)
        {
            return _values[key];
        }
        
        public T TryGetOrDefault<T>(TKey key, T defaultValue)
        {
            if(HasKey(key)) return (T)Convert.ChangeType(_values[key], typeof(T));;
            return defaultValue;
        }

        public TValue this[TKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }
        
        //Operators
        public static GoapState<TKey, TValue> operator +(GoapState<TKey, TValue> a, GoapState<TKey, TValue> b)
        {
            if (b == null) return a;
            if (a == null) return b;
            
            var propertyGroup = new GoapState<TKey, TValue>(a);
            foreach (var pair in b)
            {
                propertyGroup.Set(pair.Key, pair.Value);
            }
            return propertyGroup;
        }
        
        public static GoapState<TKey, TValue> operator +(GoapState<TKey, TValue> a, GoapEffects<TKey, TValue> b)
        {
            if (b == null) return a;
            if (a == null) return b;
            
            var propertyGroup = new GoapState<TKey, TValue>(a);
            foreach (var pair in b)
            {
                TKey key = pair.Key;
                EffectValue<TValue> bValue = pair.Value;
                
                TValue aux;
                if (propertyGroup.HasKey(key))
                {
                    aux = (TValue) EvaluateEffect(propertyGroup[key], bValue.Value, bValue.EffectType);
                }
                else
                {
                    object defValue = GetDefaultValue(bValue.Value);
                    aux = (TValue) EvaluateEffect(defValue, bValue.Value, bValue.EffectType);
                }
                propertyGroup.Set(key, aux);
            }
            return propertyGroup;
        }
        
        public static GoapState<TKey, TValue> operator -(GoapState<TKey, TValue> a, GoapBase<TKey, TValue> b)
        {
            if (b == null) return a;
            
            var propertyGroup = new GoapState<TKey, TValue>(a);
            if (b is null) return propertyGroup;
            foreach (var pair in b._values)
            {
                if(a.HasKey(pair.Key)) propertyGroup.Remove(pair.Key);
            }
            return propertyGroup;
        }
        
        //Overrides
        public override string ToString()
        {
            return _values.Aggregate("", (current, pair) => current + "Key: " + pair.Key + " | Valor: " +
                                                            pair.Value + "\n");
        }
        
        //Overrides
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            GoapState<TKey, TValue> otherPg = (GoapState<TKey, TValue>)obj;
            
            if (CountRelevantKeys() != otherPg.CountRelevantKeys()) return false;
            foreach (var key in _values.Keys)
            {
                if (!otherPg.HasKey(key)) return false;
                if(!_values[key].Equals(otherPg._values[key])) return false;
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
            foreach(KeyValuePair<TKey, TValue> kvp in _values)
            {
                //No se toman en cuenta las reglas desinformadas.
                if (!IsRelevantKey(kvp.Key)) continue;
                
                hash = 18 * hash + (kvp.Key.GetHashCode() ^ kvp.Value.GetHashCode());
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
            return _values[key].GetHashCode() != GetDefaultValue(_values[key]).GetHashCode();
        }
        #endregion
        
        //Casts
        // Implicit conversion operator
        public static implicit operator GoapState<TKey, TValue>(GoapEffects<TKey, TValue> custom)
        {
            GoapState<TKey, TValue> goapState = new GoapState<TKey, TValue>();
            goapState.Set(custom);
            return goapState;
        }
    }
}