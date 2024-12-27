using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static UGoap.Base.BaseTypes;
using static UGoap.Base.PropertyManager;

namespace UGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    public class Conditions : StateBase<HashSet<ConditionValue>>
    {
        public Conditions(Conditions conditions = null) : base(conditions)
        {
            foreach (var key in _values.Keys.ToList())
            {
                _values[key] = new HashSet<ConditionValue>(_values[key]);
            }
        }

        //Value Access
        public void Set(PropertyKey key, ConditionType conditionType, object value)
        {
            AssertValidType(key, value);
            var condition = new ConditionValue(value, conditionType);
            if (!Has(key))
            {
                HashSet<ConditionValue> conditions =
                    new HashSet<ConditionValue> { condition };
                _values[key] = conditions;
            }
            _values[key].Add(condition);
        }

        public void Set(PropertyKey key, ConditionValue value)
        {
            Set(key, value.ConditionType, value.Value);
        }

        public void Set(Conditions conditions)
        {
            foreach (var pair in conditions)
            {
                //TODO: Decide if cleaning up equals conditions should be done here.
                if (Has(pair.Key))
                {
                    bool hasEqualsCondition = false;
                    foreach (var conditionValue in Get(pair.Key))
                    {
                        if (conditionValue.ConditionType == ConditionType.Equal) hasEqualsCondition = true;
                    }

                    if (hasEqualsCondition) continue;
                }
                
                foreach (var condition in pair.Value)
                {
                    if (condition.ConditionType == ConditionType.Equal)
                    {
                        Remove(pair.Key);
                    }
                    
                    Set(pair.Key, condition.ConditionType, condition.Value);
                }
            }
        }

        public HashSet<ConditionValue> Get(PropertyKey key) => _values[key];

        public object TryGetOrDefault(PropertyKey key)
        {
            if (Has(key)) return Get(key);
            return new List<ConditionValue>
            {
                new(key.GetDefault(), ConditionType.Equal)
            };
        }

        public List<ConditionValue> TryGetOrDefault<T>(PropertyKey key, T defaultValue)
        {
            if (Has(key))
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
        public int Count => _values.Count;
        
        //GOAP Utilities, A* addons.
        public bool CheckConflict(State state)
        {
            return this.Any(pg => GetConflictCondition(pg.Key, state) != null);
        }

        public Conditions GetConflicts(State state, ICollection<PropertyKey> filter = null)
        {
            bool hasFilter = filter != null && filter.Count > 0;
            Conditions conflicts = new Conditions();
            foreach (var pair in this)
            {
                if(hasFilter && !filter.Contains(pair.Key)) continue;
                
                var condition = GetConflictCondition(pair.Key, state);
                if(condition != null) conflicts.Set(pair.Key, condition);
            }

            if (conflicts.IsEmpty()) conflicts = null;
            return conflicts;
        }

        //With the no overlap principle applied to ConditionValues no allowed values, only one should be retrieved.
        public ConditionValue GetConflictCondition(PropertyKey key, State state)
        {
            int count = 0;
            ConditionValue conflict = null;
            foreach (var condition in Get(key))
            {
                object defaultValue = key.GetDefault();
                object mainValue = !state.Has(key) ? defaultValue : state[key];

                if (!condition.Evaluate(mainValue))
                {
                    conflict = condition;
                    count++;
                }
            }

            if (count > 1)
            {
                DebugRecord.Record("[GOAP ERROR] More than one conflict found, overlapping no allowed values.");
            }

            return conflict;
        }
        
        public int CountConflicts(State state) => this.Count(pg => GetConflictCondition(pg.Key, state) != null);
        
        public Conditions Merge(Conditions goal)
        {
            if (goal == null) return this;

            foreach (var pair in this)
            {
                if (!goal.Has(pair.Key)) continue;
                var newConditions = goal[pair.Key];
                if (newConditions.Any(conditionValue => !CheckCondition(pair.Value, conditionValue)))
                    return null;
            }

            return this + goal;
        }

        public static Conditions operator +(Conditions a, Conditions b)
        {
            if (b == null) return a;
            if (a == null) return b;

            var propertyGroup = new Conditions(a);
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

            Conditions otherPg = (Conditions)obj;

            if (Count != otherPg.Count) return false;            
            foreach (var key in _values.Keys)
            {
                if (!otherPg.Has(key)) return false;
                if (!_values[key].SetEquals(otherPg._values[key])) return false;
            }
            return true;
        }

        /// <summary>
        /// Evaluate hash code of the dictionary with sort order and xor exclusion.
        /// </summary>
        /// <returns>Hash Number</returns>
        public override int GetHashCode()
        {
            int hash = 17;
            foreach(var kvp in _values)
            {
                foreach (var condition in kvp.Value)
                {
                    hash = 31 * hash + (kvp.Key.GetHashCode() ^ condition.Value.GetHashCode());
                }
            }
            return hash;
        }

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
        
        //Operators
        public Conditions ApplyEffects(Effects effects)
        {
            Conditions result = new Conditions();
            
            //If effect doesn't affect properties, they are added to result.
            foreach (var pair in this)
            {
                if (effects.Has(pair.Key)) continue;
                foreach (var condition in pair.Value)
                {
                    result.Set(pair.Key, condition);
                }
            }
            
            //Properties changed by effects.
            foreach (var effectPair in effects)
            {
                if(!Has(effectPair.Key)) continue;
                var effect = effectPair.Value;
                switch (effect.EffectType)
                {
                    case EffectType.Set:
                        foreach (var condition in Get(effectPair.Key))
                        {
                            //If no matches all conditions, effect not valid.
                            if (!condition.Evaluate(effect.Value)) return null;
                        }
                        break;
                    case EffectType.Add:
                        foreach (var condition in Get(effectPair.Key))
                        {
                            var value = Evaluate(condition.Value, EffectType.Subtract, effect.Value);
                            result.Set(effectPair.Key, condition.ConditionType, value);
                        }
                        break;
                    case EffectType.Subtract:
                        foreach (var condition in Get(effectPair.Key))
                        {
                            var value = Evaluate(condition.Value, EffectType.Add, effect.Value);
                            result.Set(effectPair.Key, condition.ConditionType, value);
                        }
                        break;
                    case EffectType.Multiply:
                        foreach (var condition in Get(effectPair.Key))
                        {
                            var value = Evaluate(condition.Value, EffectType.Divide, effect.Value);
                            result.Set(effectPair.Key, condition.ConditionType, value);
                        }
                        break;
                    case EffectType.Divide:
                        foreach (var condition in Get(effectPair.Key))
                        {
                            var value = Evaluate(condition.Value, EffectType.Multiply, effect.Value);
                            result.Set(effectPair.Key, condition.ConditionType, value);
                        }
                        break;
                }
            }

            return result;
        }
        
        public Dictionary<PropertyKey, int> GetDistances(State state, ICollection<PropertyKey> filter = null)
        {
            var distances = new Dictionary<PropertyKey, int>();
            var conflicts = GetConflicts(state, filter);

            if (filter != null)
            {
                foreach (var filterKey in filter)
                {
                    if(distances.ContainsKey(filterKey)) continue;
                    if (!state.Has(filterKey)) continue;

                    var defaultValue = filterKey.GetDefault();
                    if(state[filterKey].Equals(defaultValue)) continue;
                    
                    distances[filterKey] = GetDistance(state[filterKey],
                        ConditionType.Equal, defaultValue);
                }
            }
            
            if (conflicts == null) return distances;
            
            foreach (var pair in conflicts)
            {
                var condition = pair.Value[0];
                var value = state.TryGetOrDefault(pair.Key);
                distances[pair.Key] = condition.GetDistance(value);
            }

            return distances;
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
                            int intValue => intValue < (int)conditionValue.Value,
                            float floatValue => floatValue < (float)conditionValue.Value,
                            _ => !condition.Value.Equals(conditionValue.Value)
                        },
                        ConditionType.LessOrEqual => condition.Value switch
                        {
                            int intValue => intValue <= (int)conditionValue.Value,
                            float floatValue => floatValue <= (float)conditionValue.Value,
                            _ => condition.Value.Equals(conditionValue.Value)
                        },
                        ConditionType.GreaterThan => condition.Value switch
                        {
                            int intValue => intValue > (int)conditionValue.Value,
                            float floatValue => floatValue > (float)conditionValue.Value,
                            _ => !condition.Value.Equals(conditionValue.Value)
                        },
                        ConditionType.GreaterOrEqual => condition.Value switch
                        {
                            int intValue => intValue >= (int)conditionValue.Value,
                            float floatValue => floatValue >= (float)conditionValue.Value,
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
                            int intValue => intValue < (int)condition.Value,
                            float floatValue => floatValue < (float)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.GreaterThan => conditionValue.Value switch
                        {
                            int intValue => intValue + 1 < (int)condition.Value,
                            float floatValue => floatValue + 0.001f < (float)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.GreaterOrEqual => conditionValue.Value switch
                        {
                            int intValue => intValue < (int)condition.Value,
                            float floatValue => floatValue < (float)condition.Value,
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
                            int intValue => intValue <= (int)condition.Value,
                            float floatValue => floatValue <= (float)condition.Value,
                            _ => conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.GreaterThan => conditionValue.Value switch
                        {
                            int intValue => intValue < (int)condition.Value,
                            float floatValue => floatValue < (float)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.GreaterOrEqual => conditionValue.Value switch
                        {
                            int intValue => intValue <= (int)condition.Value,
                            float floatValue => floatValue <= (float)condition.Value,
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
                            int intValue => intValue > (int)condition.Value,
                            float floatValue => floatValue > (float)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.LessThan => conditionValue.Value switch
                        {
                            int intValue => intValue + 1 > (int)condition.Value,
                            float floatValue => floatValue + 0.001f > (float)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.LessOrEqual => conditionValue.Value switch
                        {
                            int intValue => intValue > (int)condition.Value,
                            float floatValue => floatValue > (float)condition.Value,
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
                            int intValue => intValue >= (int)condition.Value,
                            float floatValue => floatValue >= (float)condition.Value,
                            _ => conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.LessThan => conditionValue.Value switch
                        {
                            int intValue => intValue > (int)condition.Value,
                            float floatValue => floatValue > (float)condition.Value,
                            _ => !conditionValue.Value.Equals(condition.Value)
                        },
                        ConditionType.LessOrEqual => conditionValue.Value switch
                        {
                            int intValue => intValue >= (int)condition.Value,
                            float floatValue => floatValue >= (float)condition.Value,
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