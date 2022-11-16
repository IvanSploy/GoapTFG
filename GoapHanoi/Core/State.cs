using System.Collections.Generic;

namespace GoapHanoi.Core
{
    public class State<TA, TB>
    {
        private Dictionary<TA, TB> _values;

        public State()
        {
            _values = new Dictionary<TA, TB>();
        }
        
        public State(Dictionary<TA, TB> values)
        {
            _values = values;
        }
        
        //GOAP Utilities, A* addons.
        public bool IsConflictive(State<TA, TB> state, out int mismatches)
        {
            mismatches = 0;
            foreach (var pair in state._values)
            {
                if (Has(pair.Key))
                {
                    if (HasValue(pair.Key, pair.Value))
                    {
                        mismatches++;
                    }
                }
            }
            
            return mismatches == 0;
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
    }
}