using System.Collections;
using System.Collections.Generic;

namespace UGoap.Base
{
    public class GoapGoal<TKey, TValue> : IEnumerable<KeyValuePair<TKey, List<ConditionValue<TValue>>>>
    {
        //Fields
        private readonly GoapConditions<TKey, TValue> _conditions;

        //Properties
        public string Name { get; }
        public int PriorityLevel { get; }
        
        //Constructors
        public GoapGoal(string name, GoapConditions<TKey, TValue> goal, int priorityLevel)
        {
            _conditions = new GoapConditions<TKey, TValue>(goal) ;
            PriorityLevel = priorityLevel;
            Name = name;
        }
        
        public GoapGoal(GoapGoal<TKey, TValue> goapGoal)
        {
            _conditions = new GoapConditions<TKey, TValue>(goapGoal._conditions) ;
            PriorityLevel = goapGoal.PriorityLevel;
            Name = goapGoal.Name;
        }

        //Getters
        public GoapConditions<TKey, TValue> GetState()
        {
            return _conditions;
        }
        
        //GOAP Utilites
        public bool IsEmpty()
        {
            return _conditions.IsEmpty();
        }
        
        public bool IsReached (GoapState<TKey, TValue> worldGoapState)
        {
            return !_conditions.CheckConflict(worldGoapState);
        }
        
        public GoapConditions<TKey, TValue> GetConflicts (GoapState<TKey, TValue> worldGoapState)
        {
            return _conditions.GetConflict(worldGoapState);
        }
        
        public GoapConditions<TKey, TValue> ResolveFilteredGoal (GoapState<TKey, TValue> worldGoapState, GoapState<TKey, TValue> filter)
        {
            return _conditions.CheckFilteredConflict(worldGoapState, out var mismatches, filter) ? mismatches : null;
        }
        
        public int CountConflicts (GoapState<TKey, TValue> worldGoapState)
        {
            return _conditions.CountConflicts(worldGoapState);
        }

        public bool Has(TKey key)
        {
            return _conditions.HasKey(key);
        }
        
        public List<ConditionValue<T>> TryGetOrDefault<T>(TKey key, T defaultValue)
        {
            return GetState().TryGetOrDefault(key, defaultValue);
        }
        
        //Operators
        public List<ConditionValue<TValue>> this[TKey key] => GetState()[key];

        public static GoapGoal<TKey, TValue> operator +(GoapGoal<TKey, TValue> a, GoapConditions<TKey, TValue> b)
        {
            var conditionGroup = a._conditions;
            return new GoapGoal<TKey, TValue>(a.Name, conditionGroup + b, a.PriorityLevel);
        }
        
            // Implicit conversion operator
        public static implicit operator GoapConditions<TKey, TValue>(GoapGoal<TKey, TValue> goal)
        {
            return goal.GetState();
        }

        //Overrides
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (this == obj) return true;
            if (obj.GetType() != GetType()) return false;

            GoapGoal<TKey, TValue> objGoal = (GoapGoal<TKey, TValue>)obj;
            
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

        public IEnumerator<KeyValuePair<TKey, List<ConditionValue<TValue>>>> GetEnumerator()
        {
            return GetState().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetState().GetEnumerator();
        }
    }
}