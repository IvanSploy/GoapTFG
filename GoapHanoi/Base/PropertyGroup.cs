using System.Collections.Generic;
using System.Linq;

namespace GoapHanoi.Base
{
    /// <summary>
    /// A group of properties.
    /// </summary>
    /// <typeparam name="TA">Key type</typeparam>
    /// <typeparam name="TB">Value type</typeparam>
    public class PropertyGroup<TA, TB>
    {
        private readonly Dictionary<TA, TB> _values;
        
        public PropertyGroup(Dictionary<TA, TB> values = null)
        {
            //Si values es null, se crea un nuevo diccionario.
            _values = values == null ? new Dictionary<TA, TB>() : new Dictionary<TA, TB>(values);
        }

        public PropertyGroup(PropertyGroup<TA, TB> propertyGroup)
        {
            _values = new Dictionary<TA, TB>(propertyGroup._values);
        }

        //GOAP Utilities, A* addons.
        public bool CheckConflict(PropertyGroup<TA, TB> propertyGroup)
        {
            return propertyGroup._values.Where(pair => Has(pair.Key)).Any(pair => !HasValue(pair.Key, pair.Value));
        }
        
        public bool CheckConflict(PropertyGroup<TA, TB> propertyGroup, out PropertyGroup<TA, TB> mismatches)
        {
            mismatches = new PropertyGroup<TA, TB>();
            foreach (var pair in propertyGroup._values)
            {
                if (Has(pair.Key))
                {
                    if (!HasValue(pair.Key, pair.Value))
                    {
                        mismatches.Set(pair.Key, pair.Value);
                    }
                }
            }
            return !mismatches.IsEmpty();
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

        private bool Has(TA key)
        {
            return _values.ContainsKey(key);
        }
        
        private bool HasValue(TA key, TB value)
        {
            return _values[key].Equals(value);
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
            PropertyGroup<TA, TB> propertyGroup = new PropertyGroup<TA, TB>(a._values);
            foreach (var pair in b._values)
            {
                propertyGroup._values[pair.Key] = pair.Value;
            }
            
            return propertyGroup;
        }
        
        //Overrides
        public override string ToString()
        {
            return _values.Aggregate("", (current, pair) => current + ("Key: " + pair.Key + " | Valor: " + pair.Value + "\n"));
            /*var text = ""; Equivalente a la función Linq.
            foreach (var pair in _values)
            {
                text += "Key: " + pair.Key + " | Valor: " + pair.Value + "\n";
            }
            return text;*/
        }
    }
}