using System.Collections;
using System.Collections.Generic;
using UGoap.Base;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity
{
    public class UGoapGoal : IEnumerable<KeyValuePair<PropertyKey, List<ConditionValue>>>, IGoapGoal
    {
        //Properties
        public GoapConditions Conditions { get; }
        public string Name { get; }
        public int PriorityLevel { get; }
        
        //Constructors
        public UGoapGoal(string name, GoapConditions conditions, int priorityLevel)
        {
            Conditions = new GoapConditions(conditions) ;
            PriorityLevel = priorityLevel;
            Name = name;
        }
        
        public UGoapGoal(UGoapGoal goapGoal)
        {
            Conditions = new GoapConditions(goapGoal.Conditions) ;
            PriorityLevel = goapGoal.PriorityLevel;
            Name = goapGoal.Name;
        }
        
        //GOAP Utilites
        public bool IsEmpty() => Conditions.IsEmpty();
        public bool IsReached (GoapState state) => !Conditions.CheckConflict(state);
        public bool Has(PropertyKey key) => Conditions.HasKey(key);
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