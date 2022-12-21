using System.Runtime.Remoting;

namespace GoapTFG.Base
{
    public class Goal<TA, TB>
    {
        //Fields
        private PropertyGroup<TA, TB> propertyGroup;

        //Properties
        public float PriorityLevel { get; set; }
        
        //Constructors
        public Goal(PropertyGroup<TA, TB> goal, float priorityLevel)
        {
            propertyGroup = new PropertyGroup<TA, TB>(goal) ;
            PriorityLevel = priorityLevel;
        }
        
        //GOAP Utilites
        public bool IsReached (PropertyGroup<TA, TB> otherPG)
        {
            return !otherPG.CheckConflict(propertyGroup);
        }
        
        public PropertyGroup<TA, TB> GetConflicts (PropertyGroup<TA, TB> otherPG)
        {
            return otherPG.CheckConflict(propertyGroup, out var mismatches) ? mismatches : null;
        }
        
        public int CountConflicts (PropertyGroup<TA, TB> otherPG)
        {
            return otherPG.CountConflict(propertyGroup);
        }
        
        //Getters
        public PropertyGroup<TA, TB> GetState()
        {
            return propertyGroup;
        }
    }
}