using System.Collections.Generic;
using System.Linq;
using static QGoap.Base.BaseTypes;
using static QGoap.Base.PropertyManager;

namespace QGoap.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    public class ConditionGroup : Group<Condition>
    {
        public ConditionGroup(ConditionGroup conditionGroup = null) : base(conditionGroup)
        {
            foreach (var key in _values.Keys.ToList())
            {
                _values[key] = ConditionFactory.Create(_values[key]);
            }
        }
        
        public Condition Get(PropertyKey key)
        {
            _values.TryGetValue(key, out var condition);
            return condition;
        }

        public Condition this[PropertyKey key] => Get(key);
        public int Count => _values.Count;

        public void Set(PropertyKey key, ConditionType conditionType, object value)
        {
            AssertValidType(key, value);
            _values[key] = ConditionFactory.Create(conditionType, value);
        }
        
        public void Set(PropertyKey key, Condition condition)
        {
            _values[key] = condition;
        }
        
        public ConditionGroup Combine(ConditionGroup other, bool force = false)
        {
            ConditionGroup result = new ConditionGroup(this);
            if (other == null) return result;

            if (!force)
            {
                foreach (var pair in this)
                {
                    if (!other.Has(pair.Key)) continue;
                    var newCondition = other[pair.Key];
                    if (!newCondition.CheckCombine(pair.Value))
                        return null;
                }
            }
            
            foreach (var pair in other)
            {
                result.SetOrCombine(pair.Key, pair.Value);
            }
            return result;
        }
        
        public void SetOrCombine(PropertyKey key, ConditionType conditionType, object value)
        {
            AssertValidType(key, value);
            SetOrCombine(key, ConditionFactory.Create(conditionType, value));
        }
        
        public void SetOrCombine(PropertyKey key, Condition condition)
        {
            if (!Has(key))
            {
                _values[key] = condition;
                return;
            }
            Combine(key, condition);
        }
        
        public void Combine(PropertyKey key, Condition condition)
        {
            var conditionValue = Get(key);
            conditionValue?.Combine(condition);
        }
        
        //GOAP Utilities, A* addons.
        public bool HasConflict(State state)
        {
            return this.Any(pg => HasConflict(pg.Key, state));
        }

        public ConditionGroup GetConflicts(State state, ICollection<PropertyKey> filter = null)
        {
            bool hasFilter = filter != null && filter.Count > 0;
            ConditionGroup conflicts = new ConditionGroup();
            foreach (var pair in this)
            {
                if(hasFilter && !filter.Contains(pair.Key)) continue;
                
                if(HasConflict(pair.Key, state))
                {
                    conflicts.Set(pair.Key, Get(pair.Key));
                }
            }

            if (conflicts.IsEmpty()) conflicts = null;
            return conflicts;
        }

        public bool HasConflict(PropertyKey key, State state)
        {
            Condition condition = Get(key);
            object mainValue = !state.Has(key) ? key.GetDefault() : state[key];
            return !condition.Check(mainValue);
        }
        
        public int CountConflicts(State state) => this.Count(pg => HasConflict(pg.Key, state));

        //Overrides
        public override string ToString() => 
            _values.Aggregate("", (current, pair) =>
                "Key: " + pair.Key + " | Value: " + pair.Value + "\n");

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            ConditionGroup otherPg = (ConditionGroup)obj;

            if (Count != otherPg.Count) return false;            
            foreach (var key in _values.Keys)
            {
                if (!otherPg.Has(key)) return false;
                if (!_values[key].Equals(otherPg._values[key])) return false;
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
                hash = 31 * hash + (kvp.Key.GetHashCode() ^ kvp.Value.GetHashCode());
            }
            return hash;
        }
        
        //Operators
        public ConditionGroup ApplyEffects(EffectGroup effectGroup)
        {
            ConditionGroup result = new ConditionGroup();
            
            //If effect doesn't affect properties, they are added to result.
            foreach (var pair in this)
            {
                if (effectGroup.Has(pair.Key)) continue;
                result.Set(pair.Key, pair.Value);
            }
            
            //Properties changed by effects.
            foreach (var effectPair in effectGroup)
            {
                if(!Has(effectPair.Key)) continue;
                var effect = effectPair.Value;
                
                var condition = ConditionFactory.Create(Get(effectPair.Key));
                switch (effect.Type)
                {
                    case EffectType.Set:
                        if (!condition.Check(effect.Value)) return null;
                        break;
                    case EffectType.Add:
                        condition.ApplyEffect(EffectType.Subtract, effect.Value);
                        result.Set(effectPair.Key, condition);
                        break;
                    case EffectType.Subtract:
                        condition.ApplyEffect(EffectType.Add, effect.Value);
                        result.Set(effectPair.Key, condition);
                        break;
                    case EffectType.Multiply:
                        condition.ApplyEffect(EffectType.Divide, effect.Value);
                        result.Set(effectPair.Key, condition);
                        break;
                    case EffectType.Divide:
                        condition.ApplyEffect(EffectType.Multiply, effect.Value);
                        result.Set(effectPair.Key, condition);
                        break;
                }
            }

            return result;
        }
        
        public Dictionary<PropertyKey, int> GetDistances(State state, ICollection<PropertyKey> filter = null, ICollection<PropertyKey> additional = null)
        {
            var distances = new Dictionary<PropertyKey, int>();
            var conflicts = GetConflicts(state, filter);

            if (additional != null)
            {
                foreach (var additionalKey in additional)
                {
                    if(distances.ContainsKey(additionalKey)) continue;
                    if (!state.Has(additionalKey)) continue;

                    var distance = GetStateDistance(additionalKey, state[additionalKey]);
                    if(distance == 0) continue;

                    distances[additionalKey] = distance;
                }
            }
            
            if (conflicts == null) return distances;
            
            foreach (var pair in conflicts)
            {
                var condition = pair.Value;
                var value = state.TryGetOrDefault(pair.Key);
                distances[pair.Key] = condition.GetDistance(value);
            }

            return distances;
        }
    }
}