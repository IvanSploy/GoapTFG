using System;
using System.Collections.Generic;
using System.Linq;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    /// <typeparam name="PropertyKey">Key type</typeparam>
    /// <typeparam name="object">Value type</typeparam>
    public class GoapState : GoapBase<object>
    {
        public GoapState(GoapState goapState = null) : base(goapState)
        { }
        
        //Value Access
        public void Set(PropertyKey key, object value)
        {
            AssertValidType(key, value);
            _values[key] = value;
        }
        
        public void Set(GoapState otherPg)
        {
            foreach (var pair in otherPg)
            {   
                Set(pair.Key, pair.Value);
            }
        }
        
        public void Set(GoapEffects goapEffects)
        {
            foreach (var pair in goapEffects)
            {   
                Set(pair.Key, pair.Value.Value);
            }
        }
        
        private object Get(PropertyKey key)
        {
            return _values[key];
        }
        
        public object TryGetOrDefault(PropertyKey key) => Has(key) ? _values[key] : key.GetDefault();

        public T TryGetOrDefault<T>(PropertyKey key, T defaultValue)
        {
            if(Has(key)) return (T)Convert.ChangeType(_values[key], typeof(T));;
            return defaultValue;
        }

        public object this[PropertyKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }
        
        //Operators
        public static GoapState operator +(GoapState a, GoapState b)
        {
            if (b == null) return a;
            if (a == null) return b;
            
            var propertyGroup = new GoapState(a);
            foreach (var pair in b)
            {
                propertyGroup.Set(pair.Key, pair.Value);
            }
            return propertyGroup;
        }
        
        public static GoapState operator +(GoapState a, GoapEffects b)
        {
            if (b == null) return a;
            if (a == null) return b;
            
            var propertyGroup = new GoapState(a);
            foreach (var pair in b)
            {
                PropertyKey key = pair.Key;
                EffectValue bValue = pair.Value;
                
                object aux;
                if (propertyGroup.Has(key))
                {
                    aux = bValue.Evaluate(propertyGroup[key]);
                }
                else
                {
                    object defValue = key.GetDefault();
                    aux = bValue.Evaluate(defValue);
                }
                propertyGroup.Set(key, aux);
            }
            return propertyGroup;
        }
        
        public static GoapState operator -(GoapState a, GoapBase<object> b)
        {
            if (b == null) return a;
            
            var propertyGroup = new GoapState(a);
            if (b is null) return propertyGroup;
            foreach (var pair in b._values)
            {
                if(a.Has(pair.Key)) propertyGroup.Remove(pair.Key);
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

            GoapState otherPg = (GoapState)obj;
            
            if (CountRelevantPropertyKeys() != otherPg.CountRelevantPropertyKeys()) return false;
            foreach (var key in _values.Keys)
            {
                if (!otherPg.Has(key)) return false;
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
            foreach(KeyValuePair<PropertyKey, object> kvp in _values)
            {
                //No se toman en cuenta las reglas desinformadas.
                if (!IsRelevantPropertyKey(kvp.Key)) continue;
                
                hash = 18 * hash + (kvp.Key.GetHashCode() ^ kvp.Value.GetHashCode());
                hash %= int.MaxValue;
            }
            return hash;
        }
        
        #region DefaultValues
        private int CountRelevantPropertyKeys()
        {
            return _values.Keys.Count(IsRelevantPropertyKey);
        }

        private bool IsRelevantPropertyKey(PropertyKey key)
        {
            return _values[key].GetHashCode() != key.GetDefault().GetHashCode();
        }
        #endregion
        
        //Casts
        // Implicit conversion operator
        public static implicit operator GoapState(GoapEffects custom)
        {
            GoapState goapState = new GoapState();
            goapState.Set(custom);
            return goapState;
        }
    }
}