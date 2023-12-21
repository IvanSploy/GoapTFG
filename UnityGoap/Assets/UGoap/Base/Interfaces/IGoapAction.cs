using System.Collections.Generic;

namespace GoapTFG.Base
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
        PropertyGroup<TKey, TValue> GetPreconditions();
        PropertyGroup<TKey, TValue> GetEffects();
        HashSet<TKey> GetAffectedKeys();

        //GOAP utilities.
        (PropertyGroup<TKey, TValue> state, PropertyGroup<TKey, TValue> proceduralEffects) ApplyAction(GoapStateInfo<TKey, TValue> stateInfo);
        GoapStateInfo<TKey, TValue> ApplyRegressiveAction(GoapStateInfo<TKey, TValue> stateInfo, out bool reached);
        (GoapStateInfo<TKey, TValue> stateInfo, bool valid) ApplyMixedAction(PropertyGroup<TKey, TValue> state,
            GoapGoal<TKey, TValue> goal);
        PropertyGroup<TKey, TValue> Execute(GoapStateInfo<TKey, TValue> stateInfo, IGoapAgent<TKey, TValue> agent);
    }
}