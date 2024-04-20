using System.Collections.Generic;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Base
{
    public interface IGoapAction
    {
        string Name { get; }
        bool IsCompleted { get; }

        //Cost related.
        int GetCost(GoapState state, GoapGoal goal); 
        int GetCost(); 
        int SetCost(int cost);
        
        //Getters
        GoapConditions GetPreconditions(GoapStateInfo stateInfo);
        GoapEffects GetEffects(GoapStateInfo stateInfo);
        HashSet<PropertyKey> GetAffectedKeys();

        //GOAP utilities.
        (GoapState State, GoapGoal Goal) ApplyAction(GoapStateInfo info);
        (GoapState, bool) Execute(GoapStateInfo stateInfo, IGoapAgent agent);
    }
}