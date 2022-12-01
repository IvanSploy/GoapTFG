using System.Runtime.Remoting;

namespace GoapHanoi.Base
{
    public class Goal<TA, TB>
    {
        //Fields
        private PropertyGroup<TA, TB> _propertyGroup;

        //Properties
        public float PriorityLevel { get; set; }
        
        //Constructors
        public Goal(PropertyGroup<TA, TB> goal, float priorityLevel)
        {
            _propertyGroup = new PropertyGroup<TA, TB>(goal) ;
            PriorityLevel = priorityLevel;
        }
        
        //GOAP Utilites
        public bool IsReached (PropertyGroup<TA, TB> propertyGroup)
        {
            return !propertyGroup.CheckConflict(_propertyGroup);
        }
        
        public PropertyGroup<TA, TB> GetMismatches (PropertyGroup<TA, TB> propertyGroup)
        {
            return _propertyGroup.CheckConflict(propertyGroup, out var mismatches) ? mismatches : null;
        }
        
        //Getters
        public PropertyGroup<TA, TB> GetState()
        {
            return _propertyGroup;
        }
    }
}