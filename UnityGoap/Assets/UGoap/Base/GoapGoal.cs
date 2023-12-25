using System.Collections;
using System.Collections.Generic;

namespace UGoap.Base
{
    public class GoapGoal<TKey, TValue> : IEnumerable<KeyValuePair<TKey, ConditionValue<TValue>>>
    {
        //Fields
        private readonly ConditionGroup<TKey, TValue> _conditions;

        //Properties
        public string Name { get; }
        public int PriorityLevel { get; }
        
        //Constructors
        public GoapGoal(string name, ConditionGroup<TKey, TValue> goal, int priorityLevel)
        {
            _conditions = new ConditionGroup<TKey, TValue>(goal) ;
            PriorityLevel = priorityLevel;
            Name = name;
        }
        
        public GoapGoal(GoapGoal<TKey, TValue> goapGoal)
        {
            _conditions = new ConditionGroup<TKey, TValue>(goapGoal._conditions) ;
            PriorityLevel = goapGoal.PriorityLevel;
            Name = goapGoal.Name;
        }

        //Getters
        public ConditionGroup<TKey, TValue> GetState()
        {
            return _conditions;
        }
        
        //GOAP Utilites
        public bool IsEmpty()
        {
            return _conditions.IsEmpty();
        }
        
        public bool IsReached (PropertyGroup<TKey, TValue> worldState)
        {
            return !_conditions.CheckConflict(worldState);
        }
        
        public ConditionGroup<TKey, TValue> GetConflicts (PropertyGroup<TKey, TValue> worldState)
        {
            return _conditions.GetConflict(worldState);
        }
        
        public ConditionGroup<TKey, TValue> ResolveFilteredGoal (PropertyGroup<TKey, TValue> worldState, PropertyGroup<TKey, TValue> filter)
        {
            return _conditions.CheckFilteredConflict(worldState, out var mismatches, filter) ? mismatches : null;
        }
        
        public int CountConflicts (PropertyGroup<TKey, TValue> worldState)
        {
            return _conditions.CountConflict(worldState);
        }

        public bool Has(TKey key)
        {
            return _conditions.HasKey(key);
        }
        
        //Operators
        public ConditionValue<TValue> this[TKey key]
        {
            get => GetState()[key];
            set => GetState()[key] = value;
        }
        
        public static GoapGoal<TKey, TValue> operator +(GoapGoal<TKey, TValue> a, ConditionGroup<TKey, TValue> b)
        {
            var conditionGroup = a._conditions;
            return new GoapGoal<TKey, TValue>(a.Name, conditionGroup + b, a.PriorityLevel);
        }
        
            // Implicit conversion operator
        public static implicit operator ConditionGroup<TKey, TValue>(GoapGoal<TKey, TValue> goal)
        {
            return goal.GetState();
        }

        //Overrides
        public override string ToString()
        {
            return "Objetivo: " + Name + "\n" + _conditions;
        }

        public IEnumerator<KeyValuePair<TKey, ConditionValue<TValue>>> GetEnumerator()
        {
            return GetState().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetState().GetEnumerator();
        }
    }
}