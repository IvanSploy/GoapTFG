
using System;
using UnityEditor;

namespace GoapTFG.Base
{
    public class Goal<TA, TB>
    {
        //Fields
        private readonly PropertyGroup<TA, TB> _propertyGroup;

        //Properties
        public string Name { get; set; }
        public int PriorityLevel { get; set; }
        
        //Constructors
        public Goal(string name, PropertyGroup<TA, TB> goal, int priorityLevel)
        {
            _propertyGroup = new PropertyGroup<TA, TB>(goal) ;
            PriorityLevel = priorityLevel;
            Name = name;
        }
        
        //GOAP Utilites
        public bool IsReached (PropertyGroup<TA, TB> worldState)
        {
            return !worldState.CheckConflict(_propertyGroup);
        }
        
        public PropertyGroup<TA, TB> GetConflicts (PropertyGroup<TA, TB> worldState)
        {
            return worldState.CheckConflict(_propertyGroup, out var mismatches) ? mismatches : null;
        }
        
        public int CountConflicts (PropertyGroup<TA, TB> worldState)
        {
            return worldState.CountConflict(_propertyGroup);
        }
        
        //Getters
        public PropertyGroup<TA, TB> GetState()
        {
            return _propertyGroup;
        }

        //Overrides
        public override string ToString()
        {
            return "Objetivo: " + Name + "\n" + _propertyGroup;
        }
    }
}