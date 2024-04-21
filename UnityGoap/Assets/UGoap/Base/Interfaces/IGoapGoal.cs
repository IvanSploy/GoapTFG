namespace UGoap.Base
{
    /// <summary>
    /// Represents a goal, container of conditions.
    /// </summary>
    public interface IGoapGoal
    {
        //Properties
        GoapConditions Conditions { get; }
        string Name { get; }
        int PriorityLevel { get; }
        
        //Methods
        bool IsEmpty();
        bool IsReached(GoapState state);
    }
}