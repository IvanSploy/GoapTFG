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
        public EffectGroup(EffectGroup<TKey, TValue> effectGroup = null) : base(effectGroup)
        { }

        //Value Access
        public void Set(TKey key, TValue value, EffectType effectType)
        {
            Set(key, new EffectValue<TValue>(value, effectType));
        }
        
        public EffectValue<TValue> TryGetOrDefault(TKey key, TValue defaultValue)
        {
            if(HasKey(key)) return Get(key);
            else
            {
                return new EffectValue<TValue>(defaultValue, EffectType.Set);
            }
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