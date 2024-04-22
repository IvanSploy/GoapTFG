using System.Collections.Generic;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Base
{
    public interface IGoapAction
    {
        string Name { get; }
        bool IsCompleted { get; }

        //Cost related.
        int GetCost(GoapConditions goal); 
        int GetCost(); 
        int SetCost(int cost);
        
        //Getters
        GoapConditions GetPreconditions(GoapConditions goal);
        GoapEffects GetEffects(GoapConditions goal);
        HashSet<PropertyKey> GetAffectedKeys();

        //GOAP utilities.
        GoapState Execute(GoapState currentState, GoapConditions currentGoal, IGoapAgent agent);
    }
}