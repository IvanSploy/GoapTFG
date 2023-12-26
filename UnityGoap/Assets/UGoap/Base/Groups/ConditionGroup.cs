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
    public class ConditionGroup<TKey, TValue> : BaseGroup<TKey, TValue>, IEnumerable<KeyValuePair<TKey, ConditionValue<TValue>>>
    {
        public ConditionGroup(ConditionGroup<TKey, TValue> conditionGroup = null) : base(conditionGroup)
        { }
        
        //Value Access
        public void Set(TKey key, TValue value, ConditionType conditionType)
        {
            _values[key] = new ConditionValue<TValue>(value, conditionType);
        }
        
        public void Set(TKey key, ConditionValue<TValue> value)
        {
            _values[key] = new ConditionValue<TValue>(value.Value, value.ConditionType);
        }
        
        public void Set(ConditionGroup<TKey, TValue> conditionGroup)
        {
            foreach (var pair in conditionGroup)
            {   
                Set(pair.Key, pair.Value);
            }
        }
        
        public ConditionValue<TValue> Get(TKey key)
        {
            return (ConditionValue<TValue>) _values[key];
        }

        public ConditionValue<TValue> this[TKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        //GOAP Utilities, A* addons.
        public bool CheckConflict(PropertyGroup<TKey, TValue> propertyGroup)
        {
            return this.Any(pg => HasConflict(pg.Key, propertyGroup));
        }
        
        public ConditionGroup<TKey, TValue> GetConflict(PropertyGroup<TKey, TValue> propertyGroup)
        {
            ConditionGroup<TKey, TValue> mismatches = new ConditionGroup<TKey, TValue>();
            foreach (var pair in this)
            {
                if (HasConflict(pair.Key, propertyGroup))
                    mismatches.Set(pair.Key, pair.Value);
            }

            if (mismatches.IsEmpty()) mismatches = null;
            return mismatches;
        }

        public int CountConflict(PropertyGroup<TKey, TValue> propertyGroup)
        {
            return propertyGroup.Count(pg => HasConflict(pg.Key, propertyGroup));
        }

        public bool HasConflict(TKey key, PropertyGroup<TKey, TValue> propertyGroup)
        {
            object defaultValue = GetDefaultValue(Get(key).Value);
            TValue mainValue = !propertyGroup.HasKey(key) ? (TValue) defaultValue : propertyGroup[key];
                
            return !EvaluateCondition(mainValue, Get(key).Value, Get(key).ConditionType);
        }
        
        public bool CheckFilteredConflict(PropertyGroup<TKey, TValue> propertyGroup, out ConditionGroup<TKey, TValue> mismatches,
            PropertyGroup<TKey, TValue> filter)
        {
            mismatches = new ConditionGroup<TKey, TValue>();
            foreach (var pair in this)
            {
                if(!filter.HasKey(pair.Key)) mismatches.Set(pair.Key, pair.Value);
                else if (HasConflict(pair.Key, propertyGroup))
                    mismatches.Set(pair.Key, pair.Value);
            }

            var thereIsConflict = !mismatches.IsEmpty();
            if (!thereIsConflict) mismatches = null;
            return thereIsConflict;
        }
        
        public static ConditionGroup<TKey, TValue> Merge(ConditionGroup<TKey, TValue> cg, ConditionGroup<TKey, TValue> newCg)
        {
            if (cg == null) return newCg;
            if (newCg == null) return cg;
            
            foreach (var pair in cg._values)
            {
                var data = pair.Value;
                if (!newCg.HasKey(pair.Key)) continue;
                var newData = newCg[pair.Key];
                if (!newData.Equals(data))
                    return null;
            }
            return cg + newCg;
        }

        public static ConditionGroup<TKey, TValue> operator +(ConditionGroup<TKey, TValue> a, ConditionGroup<TKey, TValue> b)
        {
            if (b == null) return a;
            if (a == null) return b;
            
            var propertyGroup = new ConditionGroup<TKey, TValue>(a);
            foreach (var pair in b)
            {
                propertyGroup.Set(pair.Key, pair.Value.Value, pair.Value.ConditionType);
            }
            return propertyGroup;
        }
        
        //Overrides
        public override string ToString()
        {
            return _values.Aggregate("", (current, pair) =>
            {
                ConditionValue<TValue> conditionValue = (ConditionValue<TValue>) pair.Value;
                return current + "Key: " + pair.Key + " | Value: " +
                       conditionValue.Value + " | Condition: " + conditionValue.ConditionType + "\n";
            });
        }
        
        //Enumerable
        public IEnumerator<KeyValuePair<TKey, ConditionValue<TValue>>> GetEnumerator()
        {
            return new GroupEnumerator<ConditionValue<TValue>>(_values.Keys.ToArray(), _values.Values.Select(value =>
                (ConditionValue<TValue>)value).ToArray());
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}