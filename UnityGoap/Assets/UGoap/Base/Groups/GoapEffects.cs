using System;
using System.Linq;
using static UGoap.Base.BaseTypes;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    /// <typeparam name="PropertyKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public class GoapEffects : GoapBase< EffectValue>
    {
        public GoapEffects(GoapEffects baseGroup = null) : base(baseGroup)
        { }

        //Value Access
        public void Set(PropertyKey key, object value, EffectType effectType)
        {
            AssertValidType(key, value);
            _values[key] = new EffectValue(value, effectType);
        }
        
        public void Set(PropertyKey key, EffectValue effectValue) =>
            Set(key, effectValue.Value, effectValue.EffectType);

        public void Set(GoapEffects otherPg)
        {
            foreach (var pair in otherPg)
            {
                Set(pair.Key, pair.Value);
            }
        }
        
        public EffectValue Get(PropertyKey key) => _values[key];

        public EffectValue TryGetOrDefault<T>(PropertyKey key, T defaultValue)
        {
            if(HasKey(key))
            {
                var original = Get(key);
                return new EffectValue((T)Convert.ChangeType(original.Value, typeof(T)), original.EffectType);
            }
            return new EffectValue(defaultValue, EffectType.Set);
        }
        
        public EffectValue this[PropertyKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }
        
        //Overrides
        public override string ToString()
        {
            return _values.Aggregate("", (current, pair) =>
            {
                EffectValue effectValue = pair.Value;
                return current + "Key: " + pair.Key + " | Valor: " +
                       effectValue.Value + "\n" + " | Effect: " + effectValue.EffectType + "\n";
            });
        }
        
        public static GoapEffects operator +(GoapEffects a, GoapEffects b)
        {
            if (b == null) return a;
            if (a == null) return b;
            
            var propertyGroup = new GoapEffects(a);
            foreach (var pair in b)
            {
                propertyGroup.Set(pair.Key, pair.Value.Value, pair.Value.EffectType);
            }
            return propertyGroup;
        }
    }
}