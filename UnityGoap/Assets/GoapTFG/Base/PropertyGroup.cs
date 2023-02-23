using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

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
            public readonly Func<TB, TB, bool> Condition;
            public readonly Func<TB, TB, TB> Effect;

            public PgData(TB value)
            {
                Value = value;
                Condition = null;
                Effect = null;
            }
            
            public PgData(TB value, Func<TB, TB, bool> condition)
            {
                Value = value;
                Condition = condition;
                Effect = null;
            }
            
            public PgData(TB value, Func<TB, TB, TB> effect)
            {
                Value = value;
                Condition = null;
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
            object defaultValue = mainData.Value.GetType().Default();
            TB myValue = !HasKey(key) ? (TB) defaultValue : GetValue(key);
                
            //Se prioriza el predicado de condición de la clave en caso de que exista.
            if (mainData.Condition != null) return !mainData.Condition(myValue, mainData.Value);
            if (myValue == null) return true; //Si el valor por defecto es nulo, hay conflicto
            return !myValue.Equals(mainData.Value); //Si no son iguales, hay conflicto.
        }
        
        public bool CheckConditionsConflict(PropertyGroup<TA, TB> mainPg)
        {
            foreach (var pair in mainPg._values)
            {
                var key = pair.Key;
                if (!HasKey(pair.Key)) continue;
                if (pair.Value.Condition == null && GetCondition(key) == null)
                {
                    if (!pair.Value.Value.Equals(GetValue(key))) return true;
                    continue;
                }
                if (pair.Value.Condition != null && GetCondition(key) != null)
                {
                    if (!pair.Value.Condition(GetValue(key), pair.Value.Value)) return true;
                    continue;
                }
                   
                return true;
            }

            return false;
        }

        //Dictionary
        public void Set(TA key, TB value)
        {
            _values[key] = new PgData(value);
        }

        public void Set(TA key, TB value, Func<TB, TB, bool> condition)
        {
            _values[key] = new PgData(value, condition);
        }
        
        public void Set(TA key, TB value, Func<TB, TB, TB> effect)
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
        
        public Func<TB, TB, bool> GetCondition(TA key)
        {
            return _values[key].Condition;
        }
        
        public Func<TB, TB, TB> GetEffect(TA key)
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

        public bool IsEmpty()
        {
            return _values.Count == 0;
        }

        //Operators
        public static PropertyGroup<TA, TB> operator +(PropertyGroup<TA, TB> a, PropertyGroup<TA, TB> b)
        {
            var propertyGroup = new PropertyGroup<TA, TB>(a);
            foreach (var pair in b._values)
            {
                if (pair.Value.Effect != null)
                {
                    var aux = new PgData
                    {
                        Value = pair.Value.Effect(propertyGroup._values[pair.Key].Value, pair.Value.Value)
                    };
                    propertyGroup._values[pair.Key] = aux;
                }
                else propertyGroup._values[pair.Key] = pair.Value;
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
    }
}