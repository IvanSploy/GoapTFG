using System;
using System.Collections.Generic;
using System.Linq;

namespace GoapTFG.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    /// <typeparam name="TA">Key type</typeparam>
    /// <typeparam name="TB">Value type</typeparam>
    public class PropertyGroup<TA, TB>
    {
        private struct GpValue
        {
            public TB Value;
            public readonly Func<TB, TB, bool> Condition;
            public readonly Func<TB, TB, TB> Effect;

            public GpValue(TB value)
            {
                Value = value;
                Condition = null;
                Effect = null;
            }
            
            public GpValue(TB value, Func<TB, TB, bool> condition)
            {
                Value = value;
                Condition = condition;
                Effect = null;
            }
            
            public GpValue(TB value, Func<TB, TB, TB> effect)
            {
                Value = value;
                Condition = null;
                Effect = effect;
            }
        }

        private readonly SortedDictionary<TA, GpValue> _values;
        
        public PropertyGroup(PropertyGroup<TA, TB> propertyGroup = null)
        {
            _values = propertyGroup == null ? new SortedDictionary<TA, GpValue>()
                : new SortedDictionary<TA, GpValue>(propertyGroup._values);
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
                    mismatches.Set(pair.Key, pair.Value.Value);
            }
            return !mismatches.IsEmpty();
        }

        public int CountConflict(PropertyGroup<TA, TB> mainPg)
        {
            return mainPg._values.Count(HasConflict);
        }
        
        private bool HasConflict(KeyValuePair<TA, GpValue> mainPair)
        {
            TA key = mainPair.Key;
            if (!HasKey(key)) return true;
            //Se prioriza el predicado de condición de la clave en caso de que exista.
            if(mainPair.Value.Condition != null) return !mainPair.Value.Condition(_values[key].Value,
                mainPair.Value.Value);
            return !_values[key].Value.Equals(mainPair.Value.Value);
        }

        //Dictionary
        public void Set(TA key, TB value)
        {
            _values[key] = new GpValue(value);
        }

        public void Set(TA key, TB value, Func<TB, TB, bool> predicate)
        {
            _values[key] = new GpValue(value, predicate);
        }
        
        public void Set(TA key, TB value, Func<TB, TB, TB> effect)
        {
            _values[key] = new GpValue(value, effect);
        }
        
        public TB Get(TA key)
        {
            return _values[key].Value;
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

        public int Count()
        {
            return _values.Count;
        }
        
        //Operators
        public static PropertyGroup<TA, TB> operator +(PropertyGroup<TA, TB> a, PropertyGroup<TA, TB> b)
        {
            var propertyGroup = new PropertyGroup<TA, TB>(a);
            foreach (var pair in b._values)
            {
                if (pair.Value.Effect != null)
                {
                    var aux = new GpValue
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

            PropertyGroup<TA, TB> objPg = (PropertyGroup<TA, TB>)obj;
            return GetHashCode()==objPg.GetHashCode();
        }

        
        /// <summary>
        /// Evauate hash code of the dictionary with sort order and xor exlclusion.
        /// </summary>
        /// <returns>Hash Number</returns>
        public override int GetHashCode()
        {
            var hash = 0;
            var i = 1;
            foreach(KeyValuePair<TA, GpValue> kvp in _values)
            {
                //No se toman en cuenta las reglas desinformadas.
                if (kvp.Value.Value.GetHashCode() == 0) continue;
                
                hash ^= (kvp.Key.GetHashCode() ^ kvp.Value.Value.GetHashCode()) * i;
                i++;
            }
            return hash;
        }
    }
}