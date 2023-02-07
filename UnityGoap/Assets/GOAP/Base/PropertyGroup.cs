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
        private readonly SortedDictionary<TA, TB> _values;
        
        public PropertyGroup(SortedDictionary<TA, TB> values = null)
        {
            //Si values es null, se crea un nuevo diccionario.
            _values = values == null ? new SortedDictionary<TA, TB>() : new SortedDictionary<TA, TB>(values);
        }

        public PropertyGroup(PropertyGroup<TA, TB> propertyGroup)
        {
            _values = new SortedDictionary<TA, TB>(propertyGroup._values);
        }

        //GOAP Utilities, A* addons.
        public bool CheckConflict(PropertyGroup<TA, TB> otherPG)
        {
            return otherPG._values.Any(HasConflict);
        }
        
        public bool CheckConflict(PropertyGroup<TA, TB> otherPG, out PropertyGroup<TA, TB> mismatches)
        {
            mismatches = new PropertyGroup<TA, TB>();
            foreach (var pair in otherPG._values)
            {
                if (HasConflict(pair))
                    mismatches.Set(pair.Key, pair.Value);
            }
            return !mismatches.IsEmpty();
        }

        public int CountConflict(PropertyGroup<TA, TB> otherPG)
        {
            return otherPG._values.Count(HasConflict);
        }
        
        private bool HasConflict(KeyValuePair<TA, TB> otherPair)
        {
            if (!HasKey(otherPair.Key)) return true;
            return !CompareValue(otherPair.Key, otherPair.Value);
        }

        //Dictionary
        public void Set(TA key, TB value)
        {
            _values[key] = value;
        }
        
        public void Remove(TA key)
        {
            _values.Remove(key);
        }

        private bool HasKey(TA key)
        {
            return _values.ContainsKey(key);
        }
        
        private bool CompareValue(TA key, TB value)
        {
            return String.Compare(_values[key].ToString(), value.ToString(),
                StringComparison.Ordinal) >= 0; //Para valores no booleanos, intenta satisfacer el número en cuestión.
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
            var propertyGroup = new PropertyGroup<TA, TB>(a._values);
            foreach (var pair in b._values)
            {
                propertyGroup._values[pair.Key] = pair.Value;
            }
            
            return propertyGroup;
        }
        
        //Overrides
        public override string ToString()
        {
            return _values.Aggregate("", (current, pair) => current + "Key: " + pair.Key + " | Valor: " + pair.Value + "\n");
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
            foreach(KeyValuePair<TA, TB> kvp in _values)
            {
                //No se toman en cuenta las reglas desinformadas.
                if (kvp.Value.GetHashCode() == 0) continue;
                
                hash ^= (kvp.Key.GetHashCode() ^ kvp.Value.GetHashCode()) * i;
                i++;
            }
            return hash;
        }
    }
}