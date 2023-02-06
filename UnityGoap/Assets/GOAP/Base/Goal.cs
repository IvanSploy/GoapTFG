
namespace GoapTFG.Base
{
    public class Goal<TA, TB>
    {
        //Fields
        private readonly PropertyGroup<TA, TB> _propertyGroup;

        //Properties
        public float PriorityLevel { get; set; }
        
        //Constructors
        public Goal(PropertyGroup<TA, TB> goal, float priorityLevel)
        {
            _propertyGroup = new PropertyGroup<TA, TB>(goal) ;
            PriorityLevel = priorityLevel;
        }
        
        //GOAP Utilites
        public bool IsReached (PropertyGroup<TA, TB> otherPG)
        {
            return !otherPG.CheckConflict(_propertyGroup);
        }
        
        public PropertyGroup<TA, TB> GetConflicts (PropertyGroup<TA, TB> otherPG)
        {
            return otherPG.CheckConflict(_propertyGroup, out var mismatches) ? mismatches : null;
        }
        
        public int CountConflicts (PropertyGroup<TA, TB> otherPG)
        {
            return otherPG.CountConflict(_propertyGroup);
        }
        
        //Getters
        public PropertyGroup<TA, TB> GetState()
        {
            return _propertyGroup;
        }
    }
}