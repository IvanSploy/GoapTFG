using System.Collections;
using System.Collections.Generic;

namespace GoapTFG.Base
{
    public class GoapGoal<TValue, TKey> : IEnumerable<TValue>
    {
        //Fields
        private readonly PropertyGroup<TValue, TKey> _goalConditions;

        //Properties
        public string Name { get; }
        public int PriorityLevel { get; }
        
        //Constructors
        public GoapGoal(string name, PropertyGroup<TValue, TKey> goal, int priorityLevel)
        {
            _goalConditions = new PropertyGroup<TValue, TKey>(goal) ;
            PriorityLevel = priorityLevel;
            Name = name;
        }
        
        //GOAP Utilites
        public bool IsReached (PropertyGroup<TValue, TKey> worldState)
        {
            return !worldState.CheckConflict(_goalConditions);
        }
        
        public PropertyGroup<TValue, TKey> GetConflicts (PropertyGroup<TValue, TKey> worldState)
        {
            return worldState.CheckConflict(_goalConditions, out var mismatches) ? mismatches : null;
        }
        
        public PropertyGroup<TValue, TKey> GetFilteredConflicts (PropertyGroup<TValue, TKey> worldState, PropertyGroup<TValue, TKey> filter)
        {
            return worldState.CheckFilteredConflicts(_goalConditions, out var mismatches, filter) ? mismatches : null;
        }
        
        public int CountConflicts (PropertyGroup<TValue, TKey> worldState)
        {
            return worldState.CountConflict(_goalConditions);
        }

        public bool Has(TValue key)
        {
            return _goalConditions.Has(key);
        }

        //Getters
        public PropertyGroup<TValue, TKey> GetState()
        {
            return _goalConditions;
        }
        
        //Operators
        public TKey this[TValue key]
        {
            get => GetState()[key];
            set => GetState()[key] = value;
        }
        
        public static GoapGoal<TValue, TKey> operator +(GoapGoal<TValue, TKey> a, PropertyGroup<TValue, TKey> b)
        {
            var propertyGroup = a._goalConditions;
            return new GoapGoal<TValue, TKey>(a.Name, propertyGroup + b, a.PriorityLevel);
        }

        //Overrides

        public override string ToString()
        {
            return "Objetivo: " + Name + "\n" + _goalConditions;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return GetState().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetState().GetEnumerator();
        }
    }
}