using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using static GoapTFG.Base.BaseTypes;

namespace GoapTFG.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    /// <typeparam name="TA">Key type</typeparam>
    /// <typeparam name="TB">Value type</typeparam>
    public class PropertyGroup<TA, TB>
    {
        private struct PgData
        {
            public TB Value;
            public readonly ConditionType Condition;
            public readonly EffectType Effect;

            public PgData(TB value)
            {
                Value = value;
                Condition = default;
                Effect = default;
            }
            
            public PgData(TB value, ConditionType condition)
            {
                Value = value;
                Condition = condition;
                Effect = default;
            }
            
            public PgData(TB value, EffectType effect)
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

        private readonly SortedDictionary<TA, PgData> _values;
        
        public PropertyGroup(PropertyGroup<TA, TB> propertyGroup = null)
        {
            _values = propertyGroup == null ? new SortedDictionary<TA, PgData>()
                : new SortedDictionary<TA, PgData>(propertyGroup._values);
        }
        
        //GOAP Utilities, A* addons.
        public bool CheckConflict(PropertyGroup<TA, TB> mainPg)
        {
            return mainPg._values.Any(HasConflict);
        }
        
        public bool CheckConflict(PropertyGroup<TA, TB> mainPg, out PropertyGroup<TA, TB> mismatches)
        {
            mismatches = new PropertyGroup<TA, TB>();
            foreach (var pair in mainPg._values)
            {
                if (HasConflict(pair))
                    mismatches.Set(pair.Key, pair.Value);
            }
            return !mismatches.IsEmpty();
        }

        public int CountConflict(PropertyGroup<TA, TB> mainPg)
        {
            return mainPg._values.Count(HasConflict);
        }
        
        private bool HasConflict(KeyValuePair<TA, PgData> mainPair)
        {
            return HasConflict(mainPair.Key, mainPair.Value);
        }
        
        public bool HasConflict(TA key, PropertyGroup<TA, TB> mainPg)
        {
            return HasConflict(key, mainPg._values[key]);
        }
        
        private bool HasConflict(TA key, PgData mainData)
        {
            object defaultValue = GetDefaultValue(mainData.Value);
            TB myValue = !HasKey(key) ? (TB) defaultValue : GetValue(key);
                
            return !EvaluateCondition(myValue, mainData.Value, mainData.Condition);
        }
        
        public bool CheckConditionsConflict(PropertyGroup<TA, TB> mainPg)
        {
            foreach (var pair in mainPg._values)
            {
                var key = pair.Key;
                if (!HasKey(pair.Key)) continue;
                if (!EvaluateCondition(GetValue(key), pair.Value.Value, pair.Value.Condition)) return true;
            }

            return false;
        }

        //Dictionary
        public void Set(TA key, TB value)
        {
            _values[key] = new PgData(value);
        }

        public void Set(TA key, TB value, ConditionType condition)
        {
            _values[key] = new PgData(value, condition);
        }
        
        public void Set(TA key, TB value, EffectType effect)
        {
            _values[key] = new PgData(value, effect);
        }
        
        private void Set(TA key, PgData data)
        {
            _values[key] = new PgData(data);
        }
        
        public TB GetValue(TA key)
        {
            return _values[key].Value;
        }
        
        public ConditionType GetCondition(TA key)
        {
            return _values[key].Condition;
        }
        
        public EffectType GetEffect(TA key)
        {
            return _values[key].Effect;
        }
        
        public List<TA> GetKeys()
        {
            return new List<TA>(_values.Keys);
        }
        
        public void Remove(TA key)
        {
            _values.Remove(key);
        }

        public bool HasKey(TA key)
        {
            return _values.ContainsKey(key);
        }

        private bool IsEmpty()
        {
            return _values.Count == 0;
        }

        //Operators
        public TB this[TA key]
        {
            get => GetValue(key);
            set => Set(key, value);
        }
        
        public static PropertyGroup<TA, TB> operator +(PropertyGroup<TA, TB> a, PropertyGroup<TA, TB> b)
        {
            var propertyGroup = new PropertyGroup<TA, TB>(a);
            foreach (var pair in b._values)
            {
                var aux = new PgData();
                if (propertyGroup.HasKey(pair.Key))
                    aux.Value = (TB)EvaluateEffect(propertyGroup.GetValue(pair.Key), pair.Value.Value, pair.Value.Effect);
                else
                {
                    object defValue = GetDefaultValue(pair.Value.Value);
                    aux.Value = (TB)EvaluateEffect(defValue, pair.Value.Value, pair.Value.Effect);
                }
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

            PropertyGroup<TA, TB> otherPg = (PropertyGroup<TA, TB>)obj;
            
            if (CountRelevantKeys() != otherPg.CountRelevantKeys()) return false;
            foreach (var key in _values.Keys)
            {
                if (!otherPg.HasKey(key)) return false;
                if(!GetValue(key).Equals(otherPg.GetValue(key))) return false;
            }
            return true;
        }

        /// <summary>
        /// Evauate hash code of the dictionary with sort order and xor exlclusion.
        /// </summary>
        /// <returns>Hash Number</returns>
        public override int GetHashCode()
        {
            int hash = 18;
            foreach(KeyValuePair<TA, PgData> kvp in _values)
            {
                //No se toman en cuenta las reglas desinformadas.
                if (kvp.Value.Value.GetHashCode() == 0) continue;
                
                hash = 18 * hash + (kvp.Key.GetHashCode() ^ kvp.Value.Value.GetHashCode());
            }
            return hash;
        }
        
        private int CountRelevantKeys()
        {
            return _values.Keys.Count(key => _values[key].GetHashCode() != 0);
        }

        private static object GetDefaultValue(object value)
        {
            return value is string ? "" : value.GetType().Default();
        }
    }
}