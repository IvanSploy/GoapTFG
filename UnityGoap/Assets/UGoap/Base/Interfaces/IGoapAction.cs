using System.Collections.Generic;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Base
{
    public interface IGoapAction
    {
        string Name { get; }

        //Cost related.
        int GetCost(GoapConditions goal); 
        int GetCost(); 
        int SetCost(int cost);
        
        //Getters
        GoapConditions GetPreconditions(GoapConditions goal);
        GoapEffects GetEffects(GoapConditions goal);
        HashSet<PropertyKey> GetAffectedKeys();
        
        //Apply
        public bool Validate(GoapState state, IGoapAgent agent);
        public void Execute(ref GoapState state, IGoapAgent agent);
    }
}