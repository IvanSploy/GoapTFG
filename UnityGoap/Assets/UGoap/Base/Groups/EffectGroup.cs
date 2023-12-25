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
    public class EffectGroup<TKey, TValue> : BaseGroup<TKey, TValue>, IEnumerable<KeyValuePair<TKey, EffectValue<TValue>>>
    {
        public EffectGroup(EffectGroup<TKey, TValue> effectGroup = null) : base(effectGroup)
        { }

        //Value Access
        public void Set(TKey key, TValue value, EffectType effectType)
        {
            _values[key] = new EffectValue<TValue>(value, effectType);
        }
        
        public void Set(TKey key, EffectValue<TValue> effectValue)
        {
            _values[key] = new EffectValue<TValue>(effectValue.Value, effectValue.EffectType);
        }
        
        public void Set(EffectGroup<TKey, TValue> otherPg)
        {
            foreach (var pair in otherPg)
            {
                Set(pair.Key, pair.Value);
            }
        }
        
        public EffectValue<TValue> Get(TKey key) => (EffectValue<TValue>) _values[key];

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
                EffectValue<TValue> effectValue = (EffectValue<TValue>) pair.Value;
                return current + "Key: " + pair.Key + " | Valor: " +
                       effectValue.Value + "\n" + " | Effect: " + effectValue.EffectType + "\n";
            });
        }
        
        public static EffectGroup<TKey, TValue> operator +(EffectGroup<TKey, TValue> a, EffectGroup<TKey, TValue> b)
        {
            var propertyGroup = new EffectGroup<TKey, TValue>(a);
            foreach (var pair in b)
            {
                propertyGroup.Set(pair.Key, pair.Value.Value, pair.Value.EffectType);
            }
            return propertyGroup;
        }
        
        //Enumerable
        public IEnumerator<KeyValuePair<TKey, EffectValue<TValue>>> GetEnumerator()
        {
            return new GroupEnumerator<EffectValue<TValue>>(_values.Keys.ToArray(), _values.Values.Select(value =>
                (EffectValue<TValue>)value).ToArray());
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}