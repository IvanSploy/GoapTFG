using System;
using System.Collections.Generic;
using System.Linq;
using static QGoap.Base.PropertyManager;

namespace QGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    /// <typeparam name="PropertyKey">Key type</typeparam>
    /// <typeparam name="object">Value type</typeparam>
    public class State : Group<object>
    {
        public State(State state = null) : base(state)
        { }
        
        //Value Access
        public void Set(PropertyKey key, object value)
        {
            AssertValidType(key, value);
            _values[key] = value;
        }
        
        public void Set(State otherPg)
        {
            foreach (var pair in otherPg)
            {   
                Set(pair.Key, pair.Value);
            }
        }
        
        public void Set(EffectGroup effectGroup)
        {
            foreach (var pair in effectGroup)
            {   
                Set(pair.Key, pair.Value.Value);
            }
        }
        
        public object Get(PropertyKey key)
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
        public static State operator +(State a, State b)
        {
            if (b == null) return a;
            if (a == null) return b;
            
            var propertyGroup = new State(a);
            foreach (var pair in b)
            {
                propertyGroup.Set(pair.Key, pair.Value);
            }
            return propertyGroup;
        }
        
        public static State operator +(State a, EffectGroup b)
        {
            if (b == null) return a;
            if (a == null) return (State)b;
            
            var propertyGroup = new State(a);
            foreach (var pair in b)
            {
                PropertyKey key = pair.Key;
                Effect effect = pair.Value;
                
                object aux;
                if (propertyGroup.Has(key))
                {
                    aux = effect.Evaluate(propertyGroup[key]);
                }
                else
                {
                    object defValue = key.GetDefault();
                    aux = effect.Evaluate(defValue);
                }
                propertyGroup.Set(key, aux);
            }
            return propertyGroup;
        }
        
        public static State operator -(State a, Group<object> b)
        {
            if (b == null) return a;
            
            var propertyGroup = new State(a);
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

            State otherPg = (State)obj;
            
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
            int hash = 17;
            foreach(KeyValuePair<PropertyKey, object> kvp in _values)
            {
                if (!IsRelevantPropertyKey(kvp.Key)) continue;
                
                hash = hash * 31 + (kvp.Key.GetHashCode() ^ kvp.Value.GetHashCode());
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
        public static explicit operator State(EffectGroup custom)
        {
            State state = new State();
            state.Set(custom);
            return state;
        }
    }
}