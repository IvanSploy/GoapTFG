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
    public class ConditionGroup<TKey, TValue> : BaseGroup<TKey, HashSet<ConditionValue<TValue>>>
    {
        public ConditionGroup(ConditionGroup<TKey, TValue> conditionGroup = null) : base(conditionGroup)
        {
        }

        //Value Access
        public void Set(TKey key, TValue value, ConditionType conditionType)
        {
            var condition = new ConditionValue<TValue>(value, conditionType);
            if (!HasKey(key))
            {
                HashSet<ConditionValue<TValue>> conditions =
                    new HashSet<ConditionValue<TValue>> { condition };
                _values[key] = conditions;
            }

            _values[key].Add(condition);
        }

        public void Set(TKey key, ConditionValue<TValue> value) => Set(key, value.Value, value.ConditionType);

        public void Set(ConditionGroup<TKey, TValue> conditionGroup)
        {
            foreach (var pair in conditionGroup)
            {
                foreach (var condition in pair.Value)
                {
                    Set(pair.Key, condition.Value, condition.ConditionType);
                }
            }
        }

        public HashSet<ConditionValue<TValue>> Get(TKey key)
        {
            return _values[key];
        }

        public List<ConditionValue<TValue>> TryGetOrDefault(TKey key, TValue defaultValue)
        {
            if (HasKey(key)) return Get(key).ToList();
            return new List<ConditionValue<TValue>>
            {
                new(defaultValue, ConditionType.Equal)
            };
        }

        public List<ConditionValue<TValue>> this[TKey key] => Get(key).ToList();

        //GOAP Utilities, A* addons.
        public bool CheckConflict(StateGroup<TKey, TValue> stateGroup)
        {
            return this.Any(pg => GetConflictConditions(pg.Key, stateGroup).Count > 0);
        }

        public ConditionGroup<TKey, TValue> GetConflict(StateGroup<TKey, TValue> stateGroup)
        {
            ConditionGroup<TKey, TValue> conflicts = new ConditionGroup<TKey, TValue>();
            foreach (var pair in this)
            {
                var conditions = GetConflictConditions(pair.Key, stateGroup);
                foreach (var condition in conditions)
                {
                    conflicts.Set(pair.Key, condition);
                }
            }

            if (conflicts.IsEmpty()) conflicts = null;
            return conflicts;
        }

        public int CountConflicts(StateGroup<TKey, TValue> stateGroup) =>
            stateGroup.Count(pg => GetConflictConditions(pg.Key, stateGroup).Count > 0);

        public List<ConditionValue<TValue>> GetConflictConditions(TKey key, StateGroup<TKey, TValue> stateGroup)
        {
            List<ConditionValue<TValue>> conflicts = new List<ConditionValue<TValue>>();
            foreach (var condition in Get(key))
            {
                object defaultValue = GetDefaultValue(condition.Value);
                TValue mainValue = !stateGroup.HasKey(key) ? (TValue)defaultValue : stateGroup[key];

                if (!EvaluateCondition(mainValue, condition.Value, condition.ConditionType))
                {
                    conflicts.Add(condition);
                }
            }

            return conflicts;
        }

        public bool CheckFilteredConflict(StateGroup<TKey, TValue> stateGroup,
            out ConditionGroup<TKey, TValue> mismatches,
            StateGroup<TKey, TValue> filter)
        {
            mismatches = new ConditionGroup<TKey, TValue>();
            foreach (var pair in this)
            {
                if (!filter.HasKey(pair.Key))
                {
                    foreach (var condition in pair.Value)
                    {
                        mismatches.Set(pair.Key, condition.Value, condition.ConditionType);
                    }
                }
                else
                {
                    var conditions = GetConflictConditions(pair.Key, stateGroup);
                    foreach (var condition in conditions)
                    {
                        mismatches.Set(pair.Key, condition.Value, condition.ConditionType);
                    }
                }
            }

            var thereIsConflict = !mismatches.IsEmpty();
            if (!thereIsConflict) mismatches = null;
            return thereIsConflict;
        }

        public static ConditionGroup<TKey, TValue> Merge(ConditionGroup<TKey, TValue> cg,
            ConditionGroup<TKey, TValue> newCg)
        {
            if (cg == null) return newCg;
            if (newCg == null) return cg;

            foreach (var pair in cg._values)
            {
                var conditions = pair.Value.ToList();
                if (!newCg.HasKey(pair.Key)) continue;
                var newConditions = newCg[pair.Key];
                if (!newConditions.Equals(conditions)) //Introducir nueva formula de condiciones.
                    return null;
            }

            return cg + newCg;
        }

        public static ConditionGroup<TKey, TValue> operator +(ConditionGroup<TKey, TValue> a,
            ConditionGroup<TKey, TValue> b)
        {
            if (b == null) return a;
            if (a == null) return b;

            var propertyGroup = new ConditionGroup<TKey, TValue>(a);
            propertyGroup.Set(b);
            return propertyGroup;
        }

        //Overrides
        public override string ToString()
        {
            return _values.Aggregate("", (current, pair) =>
            {
                ConditionValue<TValue> conditionValue = (ConditionValue<TValue>)pair.Value;
                return current + "Key: " + pair.Key + " | Value: " +
                       conditionValue.Value + " | Condition: " + conditionValue.ConditionType + "\n";
            });
        }

        public override IEnumerator<KeyValuePair<TKey, HashSet<ConditionValue<TValue>>>> GetEnumerator()
        {
            return base.GetEnumerator().;
        }
    }
}