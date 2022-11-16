using System.Collections.Generic;
using System.Linq;

namespace GoapHanoi.Core
{
    public class State<TA, TB>
    {
        private readonly Dictionary<TA, TB> _values;
        
        public State(Dictionary<TA, TB> values = null)
        {
            //Si values es null, se crea un nuevo diccionario.
            _values = values == null ? new Dictionary<TA, TB>() : new Dictionary<TA, TB>(values);
        }
        
        //GOAP Utilities, A* addons.
        public bool CheckConflict(State<TA, TB> state)
        {
            return state._values.Where(pair => Has(pair.Key)).Any(pair => !HasValue(pair.Key, pair.Value));
        }
        
        public bool CheckConflict(State<TA, TB> state, out State<TA, TB> mismatches)
        {
            mismatches = new State<TA, TB>();
            foreach (var pair in state._values)
            {
                if (Has(pair.Key))
                {
                    if (!HasValue(pair.Key, pair.Value))
                    {
                        mismatches.Set(pair.Key, _values[pair.Key]);
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
        
        //Operators
        public static State<TA, TB> operator +(State<TA, TB> a, State<TA, TB> b)
        {
            State<TA, TB> state = new State<TA, TB>(a._values);
            foreach (var pair in b._values)
            {
                state._values[pair.Key] = pair.Value;
            }
            
            return state;
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