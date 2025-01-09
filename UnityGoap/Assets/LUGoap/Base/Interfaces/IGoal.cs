namespace LUGoap.Base
{
    /// <summary>
    /// Represents a goal, container of conditions.
    /// </summary>
    public interface IGoal
    {
        //Properties
        Conditions Conditions { get; }
        string Name { get; }
        int PriorityLevel { get; }
        
        //Methods
        bool IsEmpty();
        bool IsGoal(State state);
    }
}