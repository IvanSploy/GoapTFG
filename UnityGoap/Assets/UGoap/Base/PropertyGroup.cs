using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using static GoapTFG.Base.BaseTypes;

namespace GoapTFG.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    /// <typeparam name="TKey">Key type</typeparam>
    /// <typeparam name="TValue">Value type</typeparam>
    public class PropertyGroup<TKey, TValue> : IEnumerable<TKey>
    {
        private struct PgData
        {
            public TValue Value;
            public ConditionType Condition;
            public EffectType Effect;

            public PgData(TValue value)
            {
                Value = value;
                Condition = default;
                Effect = default;
            }
            
            public PgData(TValue value, ConditionType condition)
            {
                Value = value;
                Condition = condition;
                Effect = default;
            }
            
            public PgData(TValue value, EffectType effect)
            {
                Value = value;
                Condition = default;
                Effect = effect;
            }
            
            public PgData(PgData data)
            {
                Value = data.Value;
                Condition = data.Condition;
                Effect = data.Effect;
            }
        }

        private readonly SortedDictionary<TKey, PgData> _values;
        
        public PropertyGroup(PropertyGroup<TKey, TValue> propertyGroup = null)
        {
            _values = propertyGroup == null ? new SortedDictionary<TKey, PgData>()
                : new SortedDictionary<TKey, PgData>(propertyGroup._values);
        }
        
        //GOAP Utilities, A* addons.
        public bool CheckConflict(PropertyGroup<TKey, TValue> mainPg)
        {
            return mainPg._values.Any(HasConflict);
        }
        
        public bool CheckConflict(PropertyGroup<TKey, TValue> mainPg, out PropertyGroup<TKey, TValue> mismatches)
        {
            mismatches = new PropertyGroup<TKey, TValue>();
            foreach (var pair in mainPg._values)
            {
                if (HasConflict(pair))
                    mismatches.Set(pair.Key, pair.Value);
            }

            var thereIsConflict = !mismatches.IsEmpty();
            if (!thereIsConflict) mismatches = null;
            return thereIsConflict;
        }
        
        public bool CheckFilteredConflicts(PropertyGroup<TKey, TValue> mainPg, out PropertyGroup<TKey, TValue> mismatches,
            PropertyGroup<TKey, TValue> filter)
        {
            mismatches = new PropertyGroup<TKey, TValue>();
            foreach (var pair in mainPg._values)
            {
                if(!filter.Has(pair.Key)) mismatches.Set(pair.Key, pair.Value);
                if (HasConflict(pair))
                    mismatches.Set(pair.Key, pair.Value);
            }

            var thereIsConflict = !mismatches.IsEmpty();
            if (!thereIsConflict) mismatches = null;
            return thereIsConflict;
        }

        public int CountConflict(PropertyGroup<TKey, TValue> mainPg)
        {
            return mainPg._values.Count(HasConflict);
        }
        
        public bool HasConflict(TKey key, PropertyGroup<TKey, TValue> mainPg)
        {
            return HasConflict(key, mainPg._values[key]);
        }

        private bool HasConflict(KeyValuePair<TKey, PgData> mainPair)
        {
            return HasConflict(mainPair.Key, mainPair.Value);
        }

        private bool HasConflict(TKey key, PgData mainData)
        {
            object defaultValue = GetDefaultValue(mainData.Value);
            TValue myValue = !Has(key) ? (TValue) defaultValue : GetValue(key);
                
            return !EvaluateCondition(myValue, mainData.Value, mainData.Condition);
        }
        
        public bool CheckConditionsConflict(PropertyGroup<TKey, TValue> mainPg)
        {
            foreach (var pair in mainPg._values)
            {
                var key = pair.Key;
                if (!Has(pair.Key)) continue;
                if (!(GetValue(key).Equals(pair.Value.Value) && GetCondition(key).Equals(pair.Value.Condition))) return true;
            }
            return false;
        }

        //Dictionary
        public void Set(TKey key, TValue value)
        {
            _values[key] = new PgData(value);
        }

        public void Set(TKey key, TValue value, ConditionType condition)
        {
            _values[key] = new PgData(value, condition);
        }
        
        public void Set(TKey key, TValue value, EffectType effect)
        {
            _values[key] = new PgData(value, effect);
        }
        
        private void Set(TKey key, PgData data)
        {
            _values[key] = new PgData(data);
        }
        
        public void Set(PropertyGroup<TKey, TValue> otherPg)
        {
            foreach (var pair in otherPg._values)
            {   
                Set(pair.Key, pair.Value);
            }
        }
        
        public TValue GetValue(TKey key)
        {
            return _values[key].Value;
        }
        
        public ConditionType GetCondition(TKey key)
        {
            return _values[key].Condition;
        }
        
        public EffectType GetEffect(TKey key)
        {
            return _values[key].Effect;
        }
        
        public List<TKey> GetKeys()
        {
            return new List<TKey>(_values.Keys);
        }
        
        public void Remove(TKey key)
        {
            _values.Remove(key);
        }

        public bool Has(TKey key)
        {
            return _values.ContainsKey(key);
        }

        private bool IsEmpty()
        {
            return _values.Count == 0;
        }

        //Operators
        public TValue this[TKey key]
        {
            get => GetValue(key);
            set => Set(key, value);
        }
        
        public static PropertyGroup<TKey, TValue> operator +(PropertyGroup<TKey, TValue> a, PropertyGroup<TKey, TValue> b)
        {
            var propertyGroup = new PropertyGroup<TKey, TValue>(a);
            foreach (var pair in b._values)
            {
                var aux = new PgData();
                if (propertyGroup.Has(pair.Key))
                    aux.Value = (TValue)EvaluateEffect(propertyGroup.GetValue(pair.Key), pair.Value.Value, pair.Value.Effect);
                else
                {
                    object defValue = GetDefaultValue(pair.Value.Value);
                    aux.Value = (TValue)EvaluateEffect(defValue, pair.Value.Value, pair.Value.Effect);
                }
                aux.Condition = pair.Value.Condition;
                aux.Effect = pair.Value.Effect;
                propertyGroup._values[pair.Key] = aux;
            }
            
            return propertyGroup;
        }
        
        //Overrides
        public override string ToString()
        {
            return _values.Aggregate("", (current, pair) => current + "Key: " + pair.Key + " | Valor: " +
                                                            pair.Value.Value + "\n");
            /*var text = ""; Equivalente a la función Linq.
            foreach (var pair in _values)
            {
                text += "Key: " + pair.Key + " | Valor: " + pair.Value + "\n";
            }
            return text;*/
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            PropertyGroup<TKey, TValue> otherPg = (PropertyGroup<TKey, TValue>)obj;
            
            if (CountRelevantKeys() != otherPg.CountRelevantKeys()) return false;
            foreach (var key in _values.Keys)
            {
                if (!otherPg.Has(key)) return false;
                if(!GetValue(key).Equals(otherPg.GetValue(key))) return false;
            }
            return true;
        }

        /// <summary>
        /// Evaluate hash code of the dictionary with sort order and xor exclusion.
        /// </summary>
        /// <returns>Hash Number</returns>
        public override int GetHashCode()
        {
            int hash = 18;
            foreach(KeyValuePair<TKey, PgData> kvp in _values)
            {
                //No se toman en cuenta las reglas desinformadas.
                if (!IsRelevantKey(kvp.Key)) continue;
                
                hash = 18 * hash + (kvp.Key.GetHashCode() ^ kvp.Value.Value.GetHashCode());
                hash %= int.MaxValue;
            }
            return hash;
        }
        
        #region DefaultValues
        private int CountRelevantKeys()
        {
            //Posible error pero debería funcionar.
            return _values.Keys.Count(IsRelevantKey);
        }

        private bool IsRelevantKey(TKey key)
        {
            return _values[key].GetHashCode() != GetDefaultValue(_values[key]).GetHashCode();
        }

        private static object GetDefaultValue(object value)
        {
            return value is string ? "" : value.GetType().Default();
        }
        #endregion
        
        public IEnumerator<TKey> GetEnumerator()
        {
            return _values.Keys.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}