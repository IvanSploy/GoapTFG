using System;
using System.Collections;
using System.Collections.Generic;
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
    public class GoapConditions : GoapBase<HashSet<ConditionValue>>
    {
        public GoapConditions(GoapConditions goapConditions = null) : base(goapConditions)
        {
            foreach (var key in _values.Keys.ToList())
            {
                _values[key] = new HashSet<ConditionValue>(_values[key]);
            }
        }

        //Value Access
        public void Set(PropertyKey key, object value, ConditionType conditionType)
        {
            AssertValidType(key, value);
            var condition = new ConditionValue(value, conditionType);
            if (!HasKey(key))
            {
                HashSet<ConditionValue> conditions =
                    new HashSet<ConditionValue> { condition };
                _values[key] = conditions;
            }
            _values[key].Add(condition);
        }

        public void Set(PropertyKey key, ConditionValue value) => Set(key, value.Value, value.ConditionType);

        public void Set(GoapConditions goapConditions)
        {
            foreach (var pair in goapConditions)
            {
                foreach (var condition in pair.Value)
                {
                    Set(pair.Key, condition.Value, condition.ConditionType);
                }
            }
        }

        public HashSet<ConditionValue> Get(PropertyKey key)
        {
            return _values[key];
        }

        public List<ConditionValue> TryGetOrDefault<T>(PropertyKey key, T defaultValue)
        {
            if (HasKey(key))
            {
                var original = Get(key);
                List<ConditionValue> list = new List<ConditionValue>();
                foreach (var condition in original)
                {
                    list.Add(new ConditionValue((T)Convert.ChangeType(condition.Value, typeof(T)), condition.ConditionType));
                }
                return list;
            }
            return new List<ConditionValue>
            {
                new(defaultValue, ConditionType.Equal)
            };
        }

        public List<ConditionValue> this[PropertyKey key] => Get(key).ToList();

        //GOAP Utilities, A* addons.
        public bool CheckConflict(GoapState goapState)
        {
            return this.Any(pg => GetConflictConditions(pg.Key, goapState).Count > 0);
        }

        public GoapConditions GetConflict(GoapState goapState)
        {
            GoapConditions conflicts = new GoapConditions();
            foreach (var pair in this)
            {
                var conditions = GetConflictConditions(pair.Key, goapState);
                foreach (var condition in conditions)
                {
                    conflicts.Set(pair.Key, condition);
                }
            }

            if (conflicts.IsEmpty()) conflicts = null;
            return conflicts;
        }

        public int CountConflicts(GoapState goapState) =>
            this.Count(pg => GetConflictConditions(pg.Key, goapState).Count > 0);

        public List<ConditionValue> GetConflictConditions(PropertyKey key, GoapState goapState)
        {
            List<ConditionValue> conflicts = new List<ConditionValue>();
            foreach (var condition in Get(key))
            {
                object defaultValue = condition.Value.GetDefault();
                object mainValue = !goapState.HasKey(key) ? defaultValue : goapState[key];

                if (!condition.Evaluate(mainValue))
                {
                    conflicts.Add(condition);
                }
            }

            return conflicts;
        }

        public bool CheckFilteredConflict(GoapState goapState,
            out GoapConditions mismatches,
            GoapState filter)
        {
            mismatches = new GoapConditions();
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
                    var conditions = GetConflictConditions(pair.Key, goapState);
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

        public static GoapConditions Merge(GoapConditions cg,
            GoapConditions newCg)
        {
            if (cg == null) return newCg;
            if (newCg == null) return cg;

            foreach (var pair in cg)
            {
                if (!newCg.HasKey(pair.Key)) continue;
                var newConditions = newCg[pair.Key];
                if (newConditions.Any(conditionValue => !CheckCondition(pair.Value, conditionValue)))
                    return null;
            }

            return cg + newCg;
        }

        public static GoapConditions operator +(GoapConditions a,
            GoapConditions b)
        {
            if (b == null) return a;
            if (a == null) return b;

            var propertyGroup = new GoapConditions(a);
            propertyGroup.Set(b);
            return propertyGroup;
        }

        //Overrides
        public override string ToString()
        {
            return _values.Aggregate("", (current, pair) =>
            {
                string log = "";
                foreach (var condition in pair.Value)
                {
                    log += "Key: " + pair.Key + " | Value: " +
                           condition.Value + " | Condition: " + condition.ConditionType + "\n";
                }
                return log;
            });
        }
        
        //Overrides
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            GoapConditions otherPg = (GoapConditions)obj;

            if (CountRelevanPropertyKeys() != otherPg.CountRelevanPropertyKeys()) return false;            
            foreach (var key in _values.Keys)
            {
                if (!otherPg.HasKey(key)) return false;
                if (!_values[key].SetEquals(otherPg._values[key])) return false;
            }
            return true;
        }
        
        #region DefaultValues
        private int CountRelevanPropertyKeys()
        {
            return _values.Keys.Sum(key =>
                _values[key].Count(value =>
                    value.Value.GetHashCode() != value.Value.GetDefault().GetHashCode()));
        }
        #endregion

        public new IEnumerator<KeyValuePair<PropertyKey, List<ConditionValue>>> GetEnumerator()
        {
            return new ConditionsEnumerator(_values.Keys.ToArray(), _values.Values.ToArray());
        }
        
        public class ConditionsEnumerator : IEnumerator<KeyValuePair<PropertyKey, List<ConditionValue>>>
        {
            private KeyValuePair<PropertyKey, List<ConditionValue>>[] _data;
            private int _index = -1;

            public ConditionsEnumerator(PropertyKey[] keys, HashSet<ConditionValue>[] values)
            {
                _data = new KeyValuePair<PropertyKey, List<ConditionValue>>[keys.Length];
                for (var i = 0; i < keys.Length; i++)
                {
                    _data[i] = new KeyValuePair<PropertyKey, List<ConditionValue>>(keys[i], values[i].ToList());
                }
            }

            public KeyValuePair<PropertyKey, List<ConditionValue>> Current
            {
                get
                {
                    if (_index >= 0 && _index < _data.Length)
                    {
                        return _data[_index];
                    }

                    throw new InvalidOperationException();
                }
            }

            object IEnumerator.Current => Current;
            
            public bool MoveNext()
            {
                _index++;
                return _index < _data.Length;
            }

            public void Reset()
            {
                _index = -1;
            }

            public void Dispose()
            { }
        }
        
        private static bool CheckCondition(List<ConditionValue> conditions, ConditionValue conditionValue)
        {
            foreach (var condition in conditions)
            {
                bool compatible = true;
                if (condition.ConditionType == ConditionType.Equal)
                {
                    compatible = conditionValue.ConditionType switch
                    {
                        ConditionType.Equal => condition.Value.Equals(conditionValue.Value),
                        ConditionType.NotEqual => !condition.Value.Equals(conditionValue.Value),
                        ConditionType.LessThan => condition.Value switch
                        {
                            int intValue => intValue < (int)(object)conditionValue.Value,
                            float floatValue => floatValue < (float)(object)conditionValue.Value,
                            _ => !condition.Value.Equals(conditionValue.Value)
                        },
                        ConditionType.LessOrEqual => condition.Value switch
                        {
                            int intValue => intValue <= (int)(object)conditionValue.Value,
                            float floatValue => floatValue <= (float)(object)conditionValue.Value,
                            _ => condition.Value.Equals(conditionValue.Value)
                        },
                        ConditionType.GreaterThan => condition.Value switch
                        {
                            int intValue => intValue > (int)(object)conditionValue.Value,
                            float floatValue => floatValue > (float)(object)conditionValue.Value,
                            _ => !condition.Value.Equals(conditionValue.Value)
                        },
                        ConditionType.GreaterOrEqual => condition.Value switch
                        {
                            int intValue => intValue >= (int)(object)conditionValue.Value,
                            float floatValue => floatValue >= (float)(object)conditionValue.Value,
                            _ => condition.Value.Equals(conditionValue.Value)
                        },
                        _ => true
                    };
                }
                else if (condition.ConditionType == ConditionType.NotEqual)
                {
                    compatible = conditionValue.ConditionType switch
                    {
                        ConditionType.Equal => !condition.Value.Equals(conditionValue.Value),
                        ConditionType.NotEqual => condition.Value.Equals(conditionValue.Value),
                        _ => true
                    };
                }
                else if (condition.ConditionType == ConditionType.LessThan)
                {
                    compatible = conditionValue.ConditionType switch
                    {
                        ConditionType.Equal => conditionValue.Value switch
                        {
                            int intValue => intValue < (int)(object)condition.Value,
                            float floatValue => floatValue < (float)(object)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.GreaterThan => conditionValue.Value switch
                        {
                            int intValue => intValue + 1 < (int)(object)condition.Value,
                            float floatValue => floatValue + 0.1f < (float)(object)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.GreaterOrEqual => conditionValue.Value switch
                        {
                            int intValue => intValue < (int)(object)condition.Value,
                            float floatValue => floatValue < (float)(object)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        _ => true
                    };
                }
                else if (condition.ConditionType == ConditionType.LessOrEqual)
                {
                    compatible = conditionValue.ConditionType switch
                    {
                        ConditionType.Equal => conditionValue.Value switch
                        {
                            int intValue => intValue <= (int)(object)condition.Value,
                            float floatValue => floatValue <= (float)(object)condition.Value,
                            _ => conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.GreaterThan => conditionValue.Value switch
                        {
                            int intValue => intValue < (int)(object)condition.Value,
                            float floatValue => floatValue < (float)(object)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.GreaterOrEqual => conditionValue.Value switch
                        {
                            int intValue => intValue <= (int)(object)condition.Value,
                            float floatValue => floatValue <= (float)(object)condition.Value,
                            _ => conditionValue.Value.Equals(condition.Value)
                        },
                        _ => true
                    };
                }
                else if (condition.ConditionType == ConditionType.GreaterThan)
                {
                    compatible = conditionValue.ConditionType switch
                    {
                        ConditionType.Equal => conditionValue.Value switch
                        {
                            int intValue => intValue > (int)(object)condition.Value,
                            float floatValue => floatValue > (float)(object)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.LessThan => conditionValue.Value switch
                        {
                            int intValue => intValue + 1 > (int)(object)condition.Value,
                            float floatValue => floatValue + 0.1f > (float)(object)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.LessOrEqual => conditionValue.Value switch
                        {
                            int intValue => intValue > (int)(object)condition.Value,
                            float floatValue => floatValue > (float)(object)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        _ => true
                    };
                }
                else if (condition.ConditionType == ConditionType.GreaterOrEqual)
                {
                    compatible = conditionValue.ConditionType switch
                    {
                        ConditionType.Equal => conditionValue.Value switch
                        {
                            int intValue => intValue >= (int)(object)condition.Value,
                            float floatValue => floatValue >= (float)(object)condition.Value,
                            _ => conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.LessThan => conditionValue.Value switch
                        {
                            int intValue => intValue > (int)(object)condition.Value,
                            float floatValue => floatValue > (float)(object)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.LessOrEqual => conditionValue.Value switch
                        {
                            int intValue => intValue >= (int)(object)condition.Value,
                            float floatValue => floatValue >= (float)(object)condition.Value,
                            _ => conditionValue.Value.Equals(condition.Value)
                        },
                        _ => true
                    };
                }

                if (!compatible) return false;
            }
            return true;
        }
    }
}