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
    public class StateGroup<TKey, TValue> : BaseGroup<TKey, GoapValue<TValue>>
    {
        public StateGroup(StateGroup<TKey, TValue> stateGroup = null) : base(stateGroup)
        { }
        
        //Value Access
        public void Set(TKey key, TValue value)
        {
            Values[key] = new GoapValue<TValue>(value);
        }
        
        public void Set(TKey key, GoapValue<TValue> value)
        {
            Values[key] = new GoapValue<TValue>(value.Value);
        }
        
        public void Set(StateGroup<TKey, TValue> otherPg)
        {
            foreach (var pair in otherPg)
            {   
                Set(pair.Key, pair.Value);
            }
        }
        
        public void Set(ConditionGroup<TKey, TValue> otherPg)
        {
            foreach (var pair in otherPg)
            {   
                Set(pair.Key, pair.Value);
            }
        }
        
        public void Set(EffectGroup<TKey, TValue> effectGroup)
        {
            foreach (var pair in effectGroup)
            {   
                Set(pair.Key, pair.Value);
            }
        }
        
        private GoapValue<TValue> Get(TKey key)
        {
            return Values[key];
        }
        
        public GoapValue<TValue> TryGetOrDefault(TKey key, TValue defaultValue)
        {
            if(HasKey(key)) return Values[key];
            else
            {
                return new GoapValue<TValue>(defaultValue);
            }
        }

        public TValue this[TKey key]
        {
            get => Get(key).Value;
            set => Set(key, value);
        }
        
        //Operators
        public static StateGroup<TKey, TValue> operator +(StateGroup<TKey, TValue> a, StateGroup<TKey, TValue> b)
        {
            if (b == null) return a;
            if (a == null) return b;
            
            var propertyGroup = new StateGroup<TKey, TValue>(a);
            foreach (var pair in b)
            {
                propertyGroup.Set(pair.Key, pair.Value);
            }
            return propertyGroup;
        }
        
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
        
        public static StateGroup<TKey, TValue> operator -(StateGroup<TKey, TValue> a, BaseGroup<TKey, TValue> b)
        {
            if (b == null) return a;
            
            var propertyGroup = new StateGroup<TKey, TValue>(a);
            if (b is null) return propertyGroup;
            foreach (var pair in b.Values)
            {
                if(a.HasKey(pair.Key)) propertyGroup.Remove(pair.Key);
            }
            return propertyGroup;
        }
        
        //Overrides
        public override string ToString()
        {
            return Values.Aggregate("", (current, pair) => current + "Key: " + pair.Key + " | Valor: " +
                                                            pair.Value.Value + "\n");
        }
        
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