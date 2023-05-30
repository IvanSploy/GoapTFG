using System.Collections;
using System.Collections.Generic;

namespace GoapTFG.Base
{
    public class GoapGoal<TA, TB> : IEnumerable<TA>
    {
        //Fields
        private readonly PropertyGroup<TA, TB> _goalConditions;

        //Properties
        public string Name { get; }
        public int PriorityLevel { get; }
        
        //Constructors
        public GoapGoal(string name, PropertyGroup<TA, TB> goal, int priorityLevel)
        {
            _goalConditions = new PropertyGroup<TA, TB>(goal) ;
            PriorityLevel = priorityLevel;
            Name = name;
        }
        
        //GOAP Utilites
        public bool IsReached (PropertyGroup<TA, TB> worldState)
        {
            return !worldState.CheckConflict(_goalConditions);
        }
        
        public PropertyGroup<TA, TB> GetConflicts (PropertyGroup<TA, TB> worldState)
        {
            return worldState.CheckConflict(_goalConditions, out var mismatches) ? mismatches : null;
        }
        
        public PropertyGroup<TA, TB> GetFilteredConflicts (PropertyGroup<TA, TB> worldState, PropertyGroup<TA, TB> filter)
        {
            return worldState.CheckFilteredConflicts(_goalConditions, out var mismatches, filter) ? mismatches : null;
        }
        
        public int CountConflicts (PropertyGroup<TA, TB> worldState)
        {
            return worldState.CountConflict(_goalConditions);
        }

        public bool Has(TA key)
        {
            return _goalConditions.Has(key);
        }

        //Getters
        public PropertyGroup<TA, TB> GetState()
        {
            return _goalConditions;
        }
        
        //Operators
        public TB this[TA key]
        {
            get => GetState()[key];
            set => GetState()[key] = value;
        }
        
        public static GoapGoal<TA, TB> operator +(GoapGoal<TA, TB> a, PropertyGroup<TA, TB> b)
        {
            var propertyGroup = a._goalConditions;
            return new GoapGoal<TA, TB>(a.Name, propertyGroup + b, a.PriorityLevel);
        }

        //Overrides

        public override string ToString()
        {
            return "Objetivo: " + Name + "\n" + _goalConditions;
        }

        public IEnumerator<TA> GetEnumerator()
        {
            return GetState().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetState().GetEnumerator();
        }
    }
}