using System.Collections;
using System.Collections.Generic;
using UGoap.Base;
using static UGoap.Base.PropertyManager;

namespace UGoap.Unity
{
    public class UGoal : IEnumerable<KeyValuePair<PropertyKey, List<ConditionValue>>>, IGoal
    {
        //Properties
        public Conditions Conditions { get; }
        public string Name { get; }
        public int PriorityLevel { get; }
        
        //Constructors
        public UGoal(string name, Conditions conditions, int priorityLevel)
        {
            Conditions = new Conditions(conditions) ;
            PriorityLevel = priorityLevel;
            Name = name;
        }
        
        public UGoal(UGoal goal)
        {
            Conditions = new Conditions(goal.Conditions) ;
            PriorityLevel = goal.PriorityLevel;
            Name = goal.Name;
        }
        
        //GOAP Utilites
        public bool IsEmpty() => Conditions.IsEmpty();
        public bool IsGoal (State state) => !Conditions.CheckConflict(state);
        public bool Has(PropertyKey key) => Conditions.Has(key);
        public List<ConditionValue> TryGetOrDefault(PropertyKey key, object defaultValue) => 
            Conditions.TryGetOrDefault(key, defaultValue);

        //Operators
        public List<ConditionValue> this[PropertyKey key] => Conditions[key];

        //Overrides
        /*public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            UGoapGoal objGoal = (UGoapGoal)obj;
            
            return _conditions.Equals(objGoal._conditions);
        }
        
        public override int GetHashCode()
        {
            return _conditions.GetHashCode();
        }*/

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