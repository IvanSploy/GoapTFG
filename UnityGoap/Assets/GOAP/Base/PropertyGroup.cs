using System;
using System.Collections;
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
        private struct GPValue
        {
            public TB value;
            public Func<TB, TB, bool> predicate;

            public GPValue(TB value, Func<TB, TB, bool> predicate = null)
            {
                this.value = value;
                this.predicate = predicate;
            }
        }
        
        private readonly SortedDictionary<TA, GPValue> _values;
        
        public PropertyGroup(PropertyGroup<TA, TB> propertyGroup = null)
        {
            _values = propertyGroup != null ? new SortedDictionary<TA, GPValue>(propertyGroup._values)
                : new SortedDictionary<TA, GPValue>();
        }

        //GOAP Utilities, A* addons.
        public bool CheckConflict(PropertyGroup<TA, TB> mainPG)
        {
            return mainPG._values.Any(HasConflict);
        }
        
        public bool CheckConflict(PropertyGroup<TA, TB> mainPG, out PropertyGroup<TA, TB> mismatches)
        {
            mismatches = new PropertyGroup<TA, TB>();
            foreach (var pair in mainPG._values)
            {
                if (HasConflict(pair))
                    mismatches.Set(pair.Key, pair.Value.value);
            }
            return !mismatches.IsEmpty();
        }

        public int CountConflict(PropertyGroup<TA, TB> mainPG)
        {
            return mainPG._values.Count(HasConflict);
        }
        
        private bool HasConflict(KeyValuePair<TA, GPValue> mainPair)
        {
            TA key = mainPair.Key;
            if (!HasKey(key)) return true;
            //Se prioriza el predicado de la clave en caso de que exista.
            if(mainPair.Value.predicate != null) return !mainPair.Value.predicate(_values[key].value,
                mainPair.Value.value);
            return !_values[key].value.Equals(mainPair.Value.value);
        }

        //Dictionary
        public void Set(TA key, TB value, Func<TB, TB, bool> predicate = null)
        {
            _values[key] = new GPValue(value, predicate);
        }
        
        public bool SetPredicate(TA key, Func<TB, TB, bool> predicate)
        {
            if (predicate == null || !HasKey(key)) return false;
            var aux = _values[key];
            aux.predicate = predicate;
            _values[key] = aux;
            return true;
        }
        
        public TB Get(TA key)
        {
            return _values[key].value;
        }
        
        public void Remove(TA key)
        {
            _values.Remove(key);
        }

        private bool HasKey(TA key)
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
                propertyGroup._values[pair.Key] = pair.Value;
            }
            
            return propertyGroup;
        }
        
        //Overrides
        public override string ToString()
        {
            return _values.Aggregate("", (current, pair) => current + "Key: " + pair.Key + " | Valor: " +
                                                            pair.Value.value + "\n");
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
            int hash = 0;
            int i = 1;
            foreach(KeyValuePair<TA, GPValue> kvp in _values)
            {
                //No se toman en cuenta las reglas desinformadas.
                if (kvp.Value.value.GetHashCode() == 0) continue;
                
                hash ^= (kvp.Key.GetHashCode() ^ kvp.Value.value.GetHashCode()) * i;
                i++;
            }
            return hash;
        }
    }
}