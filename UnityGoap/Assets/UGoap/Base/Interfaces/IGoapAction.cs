using System.Collections.Generic;

namespace UGoap.Base
{
    public interface IGoapAction<TKey, TValue>
    {
        string Name { get; }
        bool IsCompleted { get; }

        //Cost related.
        int GetCost(GoapStateInfo<TKey, TValue> stateInfo); 
        int GetCost(); 
        int SetCost(int cost);
        
        //Getters
        ConditionGroup<TKey, TValue> GetPreconditions(GoapStateInfo<TKey, TValue> stateInfo);
        EffectGroup<TKey, TValue> GetEffects(GoapStateInfo<TKey, TValue> stateInfo);
        HashSet<TKey> GetAffectedKeys();

        //GOAP utilities.
        StateGroup<TKey, TValue> ApplyAction(GoapStateInfo<TKey, TValue> stateInfo);
        GoapStateInfo<TKey, TValue> ApplyRegressiveAction(GoapStateInfo<TKey, TValue> stateInfo, out bool reached);
        GoapStateInfo<TKey, TValue> ApplyMixedAction(StateGroup<TKey, TValue> state, GoapGoal<TKey, TValue> goal);
        StateGroup<TKey, TValue> Execute(GoapStateInfo<TKey, TValue> stateInfo, IGoapAgent<TKey, TValue> agent);
    }
}