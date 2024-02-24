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
    public class EffectGroup<TKey, TValue> : BaseGroup<TKey, EffectValue<TValue>>
    {
        public EffectGroup(EffectGroup<TKey, TValue> baseGroup = null) : base(baseGroup)
        { }

        //Value Access
        public void Set(TKey key, TValue value, EffectType effectType)
        {
            _values[key] = new EffectValue<TValue>(value, effectType);
        }
        
        public void Set(TKey key, EffectValue<TValue> effectValue) =>
            Set(key, effectValue.Value, effectValue.EffectType);

        public void Set(EffectGroup<TKey, TValue> otherPg)
        {
            foreach (var pair in otherPg)
            {
                Set(pair.Key, pair.Value);
            }
        }
        
        public EffectValue<TValue> Get(TKey key) => _values[key];

        public EffectValue<T> TryGetOrDefault<T>(TKey key, T defaultValue)
        {
            if(HasKey(key))
            {
                var original = Get(key);
                return new EffectValue<T>((T)Convert.ChangeType(original.Value, typeof(T)), original.EffectType);
            }
            return new EffectValue<T>(defaultValue, EffectType.Set);
        }
        
        public EffectValue<TValue> this[TKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }
        
        //Overrides
        public override string ToString()
        {
            return _values.Aggregate("", (current, pair) =>
            {
                EffectValue<TValue> effectValue = pair.Value;
                return current + "Key: " + pair.Key + " | Valor: " +
                       effectValue.Value + "\n" + " | Effect: " + effectValue.EffectType + "\n";
            });
        }
        
        public static EffectGroup<TKey, TValue> operator +(EffectGroup<TKey, TValue> a, EffectGroup<TKey, TValue> b)
        {
            if (b == null) return a;
            if (a == null) return b;
            
            var propertyGroup = new EffectGroup<TKey, TValue>(a);
            foreach (var pair in b)
            {
                propertyGroup.Set(pair.Key, pair.Value.Value, pair.Value.EffectType);
            }
            return propertyGroup;
        }
    }
}