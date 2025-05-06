using System;
using System.Linq;
using static QGoap.Base.BaseTypes;
using static QGoap.Base.PropertyManager;

namespace QGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    public class EffectGroup : Group<Effect>
    {
        public EffectGroup(EffectGroup groupGroup = null) : base(groupGroup)
        { }

        //Value Access
        public void Set(PropertyKey key, EffectType effectType, object value)
        {
            AssertValidType(key, value);
            effectType = AssertEffectType(key, effectType);
            _values[key] = new Effect(value, effectType);
        }
        
        public void Set(PropertyKey key, Effect effect) =>
            Set(key, effect.Type, effect.Value);

        public void Set(EffectGroup otherPg)
        {
            foreach (var pair in otherPg)
            {
                Set(pair.Key, pair.Value);
            }
        }
        
        public Effect Get(PropertyKey key) => _values[key];

        public Effect TryGetOrDefault<T>(PropertyKey key, T defaultValue)
        {
            if (!Has(key)) return new Effect(defaultValue, EffectType.Set);
            
            var original = Get(key);
            return new Effect((T)Convert.ChangeType(original.Value, typeof(T)), original.Type);
        }
        
        private EffectType AssertEffectType(PropertyKey key, EffectType type)
        {
            var propertyType = GetPropertyType(key);
            switch (propertyType)
            {
                case PropertyType.Integer:
                    if (type is EffectType.Multiply or EffectType.Divide) 
                        type = EffectType.Set;
                    break;
                case PropertyType.Float:
                    break;
                default:
                    type = EffectType.Set;
                    break;
            }

            return type;
        }
        
        public Effect this[PropertyKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }
        
        //Overrides
        public override string ToString()
        {
            return _values.Aggregate("", (current, pair) =>
            {
                Effect effect = pair.Value;
                return current + "Key: " + pair.Key + " | Effect: " + effect.Type + " | Valor: " +
                       effect.Value + "\n";
            });
        }
        
        public static EffectGroup operator +(EffectGroup a, EffectGroup b)
        {
            if (b == null) return a;
            if (a == null) return b;
            
            var propertyGroup = new EffectGroup(a);
            foreach (var pair in b)
            {
                propertyGroup.Set(pair.Key, pair.Value);
            }
            return propertyGroup;
        }
    }
}