using System.Collections;
using System.Collections.Generic;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Base
{
    public class GoapGoal : IEnumerable<KeyValuePair<PropertyKey, List<ConditionValue>>>
    {
        //Fields
        private readonly GoapConditions _conditions;

        //Properties
        public string Name { get; }
        public int PriorityLevel { get; }
        
        //Constructors
        public GoapGoal(string name, GoapConditions goal, int priorityLevel)
        {
            _conditions = new GoapConditions(goal) ;
            PriorityLevel = priorityLevel;
            Name = name;
        }
        
        public GoapGoal(GoapGoal goapGoal)
        {
            _conditions = new GoapConditions(goapGoal._conditions) ;
            PriorityLevel = goapGoal.PriorityLevel;
            Name = goapGoal.Name;
        }

        //Getters
        public GoapConditions GetState()
        {
            return _conditions;
        }
        
        //GOAP Utilites
        public bool IsEmpty()
        {
            return _conditions.IsEmpty();
        }
        
        public bool IsReached (GoapState worldGoapState)
        {
            return !_conditions.CheckConflict(worldGoapState);
        }
        
        public GoapConditions GetConflicts (GoapState worldGoapState)
        {
            return _conditions.GetConflict(worldGoapState);
        }
        
        public GoapConditions ResolveFilteredGoal (GoapState worldGoapState, GoapState filter)
        {
            return _conditions.CheckFilteredConflict(worldGoapState, out var mismatches, filter) ? mismatches : null;
        }
        
        public int CountConflicts (GoapState worldGoapState)
        {
            return _conditions.CountConflicts(worldGoapState);
        }

        public bool Has(PropertyKey key)
        {
            return _conditions.HasKey(key);
        }
        
        public List<ConditionValue> TryGetOrDefault(PropertyKey key, object defaultValue)
        {
            return GetState().TryGetOrDefault(key, defaultValue);
        }
        
        //Operators
        public List<ConditionValue> this[PropertyKey key] => GetState()[key];

        public static GoapGoal operator +(GoapGoal a, GoapConditions b)
        {
            var conditionGroup = a._conditions;
            return new GoapGoal(a.Name, conditionGroup + b, a.PriorityLevel);
        }
        
            // Implicit conversion operator
        public static implicit operator GoapConditions(GoapGoal goal)
        {
            return goal.GetState();
        }

        //Overrides
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            GoapGoal objGoal = (GoapGoal)obj;
            
            return GetState().Equals(objGoal.GetState());
        }
        
        public override int GetHashCode()
        {
            return GetState().GetHashCode();
        }

        public override string ToString()
        {
            return "Objetivo: " + Name + "\n" + _conditions;
        }

        public IEnumerator<KeyValuePair<PropertyKey, List<ConditionValue>>> GetEnumerator()
        {
            return GetState().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetState().GetEnumerator();
        }
    }
}