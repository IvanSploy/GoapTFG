using System.Collections.Generic;

namespace GoapTFG.Base
{
    public interface IGoapAction<TA, TB>
    {
        string Name { get; }
        bool IsCompleted { get; }
        IGoapAction<TA, TB> Clone();

        //Cost related.
        int GetCost(GoapStateInfo<TA, TB> stateInfo); 
        int GetCost(); 
        int SetCost(int cost);
        
        //Getters
        PropertyGroup<TA, TB> GetPreconditions();
        PropertyGroup<TA, TB> GetEffects();
        HashSet<TA> GetAffectedEffects();

        //GOAP utilities.
        PropertyGroup<TA, TB> ApplyAction(GoapStateInfo<TA, TB> stateInfo);
        GoapStateInfo<TA, TB> ApplyRegressiveAction(GoapStateInfo<TA, TB> stateInfo, out bool reached);
        PropertyGroup<TA, TB> Execute(PropertyGroup<TA, TB> worldState, IGoapAgent<TA, TB> agent);
    }
}