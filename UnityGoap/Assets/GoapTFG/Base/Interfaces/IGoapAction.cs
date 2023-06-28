using System.Collections.Generic;

namespace GoapTFG.Base
{
    public interface IGoapAction<TKey, TValue>
    {
        string Name { get; }
        bool IsCompleted { get; }
        
        IGoapAction<TKey, TValue> Clone();

        //Cost related.
        int GetCost(GoapStateInfo<TKey, TValue> stateInfo); 
        int GetCost(); 
        int SetCost(int cost);
        
        //Getters
        PropertyGroup<TKey, TValue> GetPreconditions();
        PropertyGroup<TKey, TValue> GetEffects();
        HashSet<TKey> GetAffectedKeys();

        //GOAP utilities.
        PropertyGroup<TKey, TValue> ApplyAction(GoapStateInfo<TKey, TValue> stateInfo);
        GoapStateInfo<TKey, TValue> ApplyRegressiveAction(GoapStateInfo<TKey, TValue> stateInfo, out bool reached);
        PropertyGroup<TKey, TValue> Execute(PropertyGroup<TKey, TValue> worldState, IGoapAgent<TKey, TValue> agent);
    }
}