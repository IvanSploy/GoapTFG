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
    public class ConditionGroup<TKey, TValue> : BaseGroup<TKey, List<ConditionValue<TValue>>>
    {
        public ConditionGroup(ConditionGroup<TKey, TValue> conditionGroup = null) : base(conditionGroup)
        { }
        
        //Value Access
        public void Set(TKey key, TValue value, ConditionType conditionType)
        {
            Set(key, new ConditionValue<TValue>(value, conditionType));
        }
        
        private void Set(TKey key, ConditionValue<TValue> value)
        {
            if (!_values.ContainsKey(key))
            {
                _values[key] = new List<ConditionValue<TValue>>();
            }
            _values[key].Add(new ConditionValue<TValue>(value.Value, value.ConditionType));
        }
        
        protected override void Set(TKey key, List<ConditionValue<TValue>> listValue)
        {
            _values[key] = new List<ConditionValue<TValue>>(listValue);
        }

        public List<ConditionValue<TValue>> TryGetOrDefault(TKey key, TValue defaultValue)
        {
            if(HasKey(key)) return Get(key);
            return new List<ConditionValue<TValue>> { new(defaultValue, ConditionType.Equal) };
        }
        
        //GOAP Utilities, A* addons.
        public bool CheckConflict(StateGroup<TKey, TValue> stateGroup)
        {
            return _values.Any(pg => HasConflict(pg.Key, stateGroup));
        }
        
        public int CountConflict(StateGroup<TKey, TValue> stateGroup)
        {
            return _values.Count(pg => HasConflict(pg.Key, stateGroup));
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
        
        public ConditionGroup<TKey, TValue> GetConflict(StateGroup<TKey, TValue> stateGroup)
        {
            ConditionGroup<TKey, TValue> mismatches = new ConditionGroup<TKey, TValue>();
            foreach (var pair in _values)
            {
                if (HasConflict(pair.Key, stateGroup))
                    mismatches.Set(pair.Key, pair.Value);
            }

            if (mismatches.IsEmpty()) mismatches = null;
            return mismatches;
        }

        public bool HasConflict(TKey key, StateGroup<TKey, TValue> stateGroup)
        {
            object defaultValue = GetDefaultValue(Get(key)[0].Value);
            TValue mainValue = !stateGroup.HasKey(key) ? (TValue) defaultValue : stateGroup[key].Value;
            
            foreach (var conditionValue in Get(key))
            {
                if (!EvaluateCondition(mainValue, conditionValue.Value, conditionValue.ConditionType))
                    return true;
            }
            return false;
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
                if (propertyGroup.HasKey(pair.Key))
                {
                    foreach (var bValue in pair.Value.Where(bValue => !propertyGroup.Get(pair.Key).Any(aValue =>
                                 aValue.ConditionType == bValue.ConditionType &&
                                 aValue.Value.Equals(bValue.Value))))
                    {
                        propertyGroup.Set(pair.Key, bValue.Value, bValue.ConditionType);
                    }
                }
                else
                {
                    propertyGroup.Set(pair.Key, pair.Value);
                }
            }
            return propertyGroup;
        }
        
        //Overrides
        public override string ToString()
        {
            return _values.Aggregate("", (current, pair) =>
            {
                List<ConditionValue<TValue>> conditionValues = pair.Value;
                string log = current + "Key: " + pair.Key + "\n";
                foreach (var conditionValue in conditionValues)
                {
                    log += "\t| Value: " +
                           conditionValue.Value + " | Condition: " + conditionValue.ConditionType + "\n";
                }
                return log;
            });
        }
    }
}