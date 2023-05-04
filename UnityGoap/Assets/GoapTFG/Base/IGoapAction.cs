namespace GoapTFG.Base
{
    public interface IGoapAction<TA, TB>
    {
        string Name { get; }
        bool IsCompleted { get; }

        //Cost related.
        int GetCost(); 
        int SetCost(int cost);
        
        //Getters
        PropertyGroup<TA, TB> GetPreconditions();
        PropertyGroup<TA, TB> GetEffects();

        //GOAP utilities.
        PropertyGroup<TA, TB> ApplyAction(PropertyGroup<TA, TB> worldState);
        PropertyGroup<TA, TB> Execute(PropertyGroup<TA, TB> worldState);
    }
}