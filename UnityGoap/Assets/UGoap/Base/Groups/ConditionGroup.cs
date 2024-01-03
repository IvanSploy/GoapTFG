using System.Linq;
using static UGoap.Base.BaseTypes;

namespace UGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public class ConditionGroup<TKey, TValue> : BaseGroup<TKey, ConditionValue<TValue>>
    {
        public ConditionGroup(ConditionGroup<TKey, TValue> conditionGroup = null) : base(conditionGroup)
        { }
        
        //Value Access
        public void Set(TKey key, TValue value, ConditionType conditionType)
        {
            Values[key] = new ConditionValue<TValue>(value, conditionType);
        }
        
        public void Set(TKey key, ConditionValue<TValue> value)
        {
            Values[key] = new ConditionValue<TValue>(value.Value, value.ConditionType);
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
            return (ConditionValue<TValue>) Values[key];
        }

        public ConditionValue<TValue> TryGetOrDefault(TKey key, TValue defaultValue)
        {
            if(HasKey(key)) return Get(key);
            else
            {
                return new ConditionValue<TValue>(defaultValue, ConditionType.Equal);
            }
        }
        
        public ConditionValue<TValue> this[TKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        //GOAP Utilities, A* addons.
        public bool CheckConflict(StateGroup<TKey, TValue> stateGroup)
        {
            return this.Any(pg => HasConflict(pg.Key, stateGroup));
        }
        
        public ConditionGroup<TKey, TValue> GetConflict(StateGroup<TKey, TValue> stateGroup)
        {
            ConditionGroup<TKey, TValue> mismatches = new ConditionGroup<TKey, TValue>();
            foreach (var pair in this)
            {
                if (HasConflict(pair.Key, stateGroup))
                    mismatches.Set(pair.Key, pair.Value);
            }

            if (mismatches.IsEmpty()) mismatches = null;
            return mismatches;
        }

        public int CountConflict(StateGroup<TKey, TValue> stateGroup)
        {
            return stateGroup.Count(pg => HasConflict(pg.Key, stateGroup));
        }

        public bool HasConflict(TKey key, StateGroup<TKey, TValue> stateGroup)
        {
            object defaultValue = GetDefaultValue(Get(key).Value);
            TValue mainValue = !stateGroup.HasKey(key) ? (TValue) defaultValue : stateGroup[key];
                
            return !EvaluateCondition(mainValue, Get(key).Value, Get(key).ConditionType);
        }
        
        public bool CheckFilteredConflict(StateGroup<TKey, TValue> stateGroup, out ConditionGroup<TKey, TValue> mismatches,
            StateGroup<TKey, TValue> filter)
        {
            mismatches = new ConditionGroup<TKey, TValue>();
            foreach (var pair in this)
            {
                if(!filter.HasKey(pair.Key)) mismatches.Set(pair.Key, pair.Value);
                else if (HasConflict(pair.Key, stateGroup))
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
            
            foreach (var pair in cg.Values)
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
            return Values.Aggregate("", (current, pair) =>
            {
                ConditionValue<TValue> conditionValue = (ConditionValue<TValue>) pair.Value;
                return current + "Key: " + pair.Key + " | Value: " +
                       conditionValue.Value + " | Condition: " + conditionValue.ConditionType + "\n";
            });
        }
    }
}