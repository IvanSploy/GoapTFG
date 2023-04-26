namespace GoapTFG.Base
{
    public class GoapGoal<TA, TB>
    {
        //Fields
        private readonly PropertyGroup<TA, TB> _propertyGroup;

        //Properties
        public string Name { get; set; }
        public int PriorityLevel { get; set; }
        public event GoapAction<TA, TB>.Condition ProceduralConditions; //Used in regressive Search.
        
        //Constructors
        public GoapGoal(string name, PropertyGroup<TA, TB> goal, int priorityLevel)
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

        public bool CheckProcedural(IAgent<TA, TB> agent, PropertyGroup<TA, TB> pg)
        {
            return ProceduralConditions == null || ProceduralConditions.Invoke(agent, pg);
        }
        
        //Getters
        public PropertyGroup<TA, TB> GetState()
        {
            return _propertyGroup;
        }
        
        //Operators
        public static GoapGoal<TA, TB> operator +(GoapGoal<TA, TB> a, PropertyGroup<TA, TB> b)
        {
            var propertyGroup = a._propertyGroup;
            return new GoapGoal<TA, TB>(a.Name, propertyGroup + b, a.PriorityLevel);
        }

        //Overrides
        public override string ToString()
        {
            return "Objetivo: " + Name + "\n" + _propertyGroup;
        }
    }
}