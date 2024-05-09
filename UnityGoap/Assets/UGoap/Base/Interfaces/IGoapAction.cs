using System.Collections.Generic;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Base
{
    public interface IGoapAction
    {
        //Definition.
        string Name { get; }
        string GetName(GoapConditions conditions, GoapEffects effects);
        GoapConditions GetPreconditions(GoapSettings settings);
        GoapEffects GetEffects(GoapSettings settings);
        HashSet<PropertyKey> GetAffectedKeys();

        //Cost related.
        int GetCost(GoapConditions goal); 
        int GetCost(); 
        int SetCost(int cost);
        
        //Apply
        public bool Validate(GoapState state, GoapActionInfo actionInfo, IGoapAgent agent);
        public void Execute(ref GoapState state, IGoapAgent agent);
    }
}