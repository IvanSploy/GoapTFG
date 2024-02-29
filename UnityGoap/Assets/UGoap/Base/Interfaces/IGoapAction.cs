using System.Collections.Generic;

namespace UGoap.Base
{
    public interface IGoapAction<TKey, TValue>
    {
        string Name { get; }
        bool IsCompleted { get; }

        //Cost related.
        int GetCost(GoapState<TKey, TValue> state, GoapGoal<TKey,TValue> goal); 
        int GetCost(); 
        int SetCost(int cost);
        
        //Getters
        GoapConditions<TKey, TValue> GetPreconditions(GoapStateInfo<TKey, TValue> stateInfo);
        GoapEffects<TKey, TValue> GetEffects(GoapStateInfo<TKey, TValue> stateInfo);
        HashSet<TKey> GetAffectedKeys();

        //GOAP utilities.
        (GoapState<TKey, TValue> State, GoapGoal<TKey,TValue> Goal) ApplyAction(GoapStateInfo<TKey, TValue> info);
        GoapState<TKey, TValue> Execute(GoapStateInfo<TKey, TValue> stateInfo, IGoapAgent<TKey, TValue> agent);
    }
}