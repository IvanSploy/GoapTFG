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
    public class PropertyGroup<TKey, TValue> : BaseGroup<TKey, TValue>, IEnumerable<KeyValuePair<TKey, GoapValue<TValue>>>
    {
        public PropertyGroup(PropertyGroup<TKey, TValue> propertyGroup = null) : base(propertyGroup)
        { }
        
        //Value Access
        public void Set(TKey key, TValue value)
        {
            _values[key] = new GoapValue<TValue>(value);
        }
        
        public void Set(TKey key, GoapValue<TValue> value)
        {
            _values[key] = new GoapValue<TValue>(value.Value);
        }
        
        public void Set(PropertyGroup<TKey, TValue> otherPg)
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
            return _values[key];
        }

        public TValue this[TKey key]
        {
            get => Get(key).Value;
            set => Set(key, value);
        }
        
        //Operators
        public static PropertyGroup<TKey, TValue> operator +(PropertyGroup<TKey, TValue> a, PropertyGroup<TKey, TValue> b)
        {
            var propertyGroup = new PropertyGroup<TKey, TValue>(a);
            foreach (var pair in b)
            {
                propertyGroup.Set(pair.Key, pair.Value);
            }
            return propertyGroup;
        }
        
        public static PropertyGroup<TKey, TValue> operator +(PropertyGroup<TKey, TValue> a, EffectGroup<TKey, TValue> b)
        {
            var propertyGroup = new PropertyGroup<TKey, TValue>(a);
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
        
        public static PropertyGroup<TKey, TValue> operator -(PropertyGroup<TKey, TValue> a, BaseGroup<TKey, TValue> b)
        {
            var propertyGroup = new PropertyGroup<TKey, TValue>(a);
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
                                                            pair.Value.Value + "\n");
        }
        
        //Enumerable
        public IEnumerator<KeyValuePair<TKey, GoapValue<TValue>>> GetEnumerator()
        {
            return new GroupEnumerator<GoapValue<TValue>>(_values.Keys.ToArray(), _values.Values.ToArray());
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        //Casts
        // Implicit conversion operator
        public static implicit operator PropertyGroup<TKey, TValue>(ConditionGroup<TKey, TValue> custom)
        {
            PropertyGroup<TKey, TValue> propertyGroup = new PropertyGroup<TKey, TValue>();
            propertyGroup.Set(custom);
            return propertyGroup;
        }
        
        // Implicit conversion operator
        public static implicit operator PropertyGroup<TKey, TValue>(EffectGroup<TKey, TValue> custom)
        {
            PropertyGroup<TKey, TValue> propertyGroup = new PropertyGroup<TKey, TValue>();
            propertyGroup.Set(custom);
            return propertyGroup;
        }
    }
}