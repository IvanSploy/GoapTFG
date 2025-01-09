using System;
using System.Linq;
using static LUGoap.Base.BaseTypes;
using static LUGoap.Base.PropertyManager;

namespace LUGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    public class Effects : StateBase<EffectValue>
    {
        public Effects(Effects baseGroup = null) : base(baseGroup)
        { }

        //Value Access
        public void Set(PropertyKey key, EffectType effectType, object value)
        {
            AssertValidType(key, value);
            _values[key] = new EffectValue(value, effectType);
        }
        
        public void Set(PropertyKey key, EffectValue effectValue) =>
            Set(key, effectValue.EffectType, effectValue.Value);

        public void Set(Effects otherPg)
        {
            foreach (var pair in otherPg)
            {
                Set(pair.Key, pair.Value);
            }
        }
        
        public EffectValue Get(PropertyKey key) => _values[key];

        public EffectValue TryGetOrDefault<T>(PropertyKey key, T defaultValue)
        {
            if (!Has(key)) return new EffectValue(defaultValue, EffectType.Set);
            
            var original = Get(key);
            return new EffectValue((T)Convert.ChangeType(original.Value, typeof(T)), original.EffectType);
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
                return current + "Key: " + pair.Key + " | Effect: " + effectValue.EffectType + " | Valor: " +
                       effectValue.Value + "\n";
            });
        }
        
        public static Effects operator +(Effects a, Effects b)
        {
            if (b == null) return a;
            if (a == null) return b;
            
            var propertyGroup = new Effects(a);
            foreach (var pair in b)
            {
                propertyGroup.Set(pair.Key, pair.Value);
            }
            return propertyGroup;
        }
    }
}