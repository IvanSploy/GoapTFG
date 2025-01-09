using System.Collections;
using System.Collections.Generic;
using static LUGoap.Base.PropertyManager;

namespace LUGoap.Base
{
    public class Goal : IEnumerable<KeyValuePair<PropertyKey, List<ConditionValue>>>, IGoal
    {
        //Properties
        public Conditions Conditions { get; }
        public string Name { get; }
        public int PriorityLevel { get; }
        
        //Constructors
        public Goal(string name, Conditions conditions, int priorityLevel)
        {
            Conditions = new Conditions(conditions) ;
            PriorityLevel = priorityLevel;
            Name = name;
        }
        
        //GOAP Utilites
        public bool IsEmpty() => Conditions.IsEmpty();
        public bool IsGoal (State state) => !Conditions.CheckConflict(state);
        public bool Has(PropertyKey key) => Conditions.Has(key);
        public List<ConditionValue> TryGetOrDefault(PropertyKey key, object defaultValue) => 
            Conditions.TryGetOrDefault(key, defaultValue);

        //Operators
        public List<ConditionValue> this[PropertyKey key] => Conditions[key];

        public override string ToString()
        {
            return "Goal: " + Name + "\n" + Conditions;
        }

        public IEnumerator<KeyValuePair<PropertyKey, List<ConditionValue>>> GetEnumerator()
        {
            return Conditions.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Conditions.GetEnumerator();
        }
    }
}