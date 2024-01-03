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
    public class StateGroup<TKey, TValue> : BaseGroup<TKey, StateValue<TValue>>
    {
        public StateGroup(StateGroup<TKey, TValue> stateGroup = null) : base(stateGroup)
        { }
        
        //Value Access
        public void Set(TKey key, TValue value)
        {
            _values[key] = new StateValue<TValue>(value);
        }
        
        public void Set(ConditionGroup<TKey, TValue> otherPg)
        {
            foreach (var pair in otherPg)
            {
                foreach (var conditionValue in pair.Value)
                {
                    Set(pair.Key, conditionValue);
                }
            }
        }
        
        public void Set(EffectGroup<TKey, TValue> otherPg)
        {
            foreach (var pair in otherPg)
            {
                Set(pair.Key, pair.Value);
            }
        }
        
        public StateValue<TValue> TryGetOrDefault(TKey key, TValue defaultValue)
        {
            if(HasKey(key)) return _values[key];
            else
            {
                return new StateValue<TValue>(defaultValue);
            }
        }
        
        //Operators
        
        public static StateGroup<TKey, TValue> operator +(StateGroup<TKey, TValue> a, EffectGroup<TKey, TValue> b)
        {
            if (b == null) return a;
            if (a == null) return b;
            
            var propertyGroup = new StateGroup<TKey, TValue>(a);
            foreach (var pair in b)
            {
                TKey key = pair.Key;
                EffectValue<TValue> bValue = pair.Value;
                
                TValue aux;
                if (propertyGroup.HasKey(key))
                {
                    aux = (TValue) EvaluateEffect(propertyGroup[key].Value, bValue.Value, bValue.EffectType);
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
        
        //Overrides
        public override string ToString()
        {
            return _values.Aggregate("", (current, pair) => current + "Key: " + pair.Key + " | Valor: " +
                                                            pair.Value.Value + "\n");
        }
        
        //Overrides
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            StateGroup<TKey, TValue> otherPg = (StateGroup<TKey, TValue>)obj;
            
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
            foreach(KeyValuePair<TKey, StateValue<TValue>> kvp in _values)
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
        
        //Casts
        // Implicit conversion operator
        public static implicit operator StateGroup<TKey, TValue>(ConditionGroup<TKey, TValue> custom)
        {
            StateGroup<TKey, TValue> stateGroup = new StateGroup<TKey, TValue>();
            stateGroup.Set(custom);
            return stateGroup;
        }
        
        // Implicit conversion operator
        public static implicit operator StateGroup<TKey, TValue>(EffectGroup<TKey, TValue> custom)
        {
            StateGroup<TKey, TValue> stateGroup = new StateGroup<TKey, TValue>();
            stateGroup.Set(custom);
            return stateGroup;
        }
    }
}