using System.Collections.Generic;
using Unity.VisualScripting;

namespace GoapTFG.Base
{
    public abstract class GoapAction<TKey, TValue> : IGoapAction<TKey, TValue>
    {
        //Fields
        public string Name { get; private set; }
        private PropertyGroup<TKey, TValue> _preconditions;
        private PropertyGroup<TKey, TValue> _effects;
        private PropertyGroup<TKey, TValue> _proceduralEffects;
        private int _cost = 1;
        public bool IsCompleted { get; } = false;

        //Creation of the scriptable TValue
        protected GoapAction()
        {
            _preconditions = new();
            _effects = new();
        }

        //Procedural related.
        protected abstract bool ProceduralConditions(GoapStateInfo<TKey, TValue> stateInfo);
        protected abstract PropertyGroup<TKey, TValue> GetProceduralEffects(GoapStateInfo<TKey, TValue> stateInfo);
        protected abstract void PerformedActions(IGoapAgent<TKey, TValue> agent);
        
        //Cost related.
        public int GetCost() => _cost;
        public abstract IGoapAction<TKey, TValue> Clone();

        public virtual int GetCost(GoapStateInfo<TKey, TValue> stateInfo) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public PropertyGroup<TKey, TValue> GetPreconditions() => _preconditions;
        public PropertyGroup<TKey, TValue> GetEffects() => _effects;
        public HashSet<TKey> GetAffectedKeys()
        {
            HashSet<TKey> affectedPropertyLists = new HashSet<TKey>();
            affectedPropertyLists.AddRange(_effects.GetKeys());
            return affectedPropertyLists;
        }

        //GOAP utilities.
        public PropertyGroup<TKey, TValue> ApplyAction(GoapStateInfo<TKey, TValue> stateInfo)
        {
            if (!CheckAction(stateInfo)) return null;
            return DoApplyAction(stateInfo);
        }
        
        public GoapStateInfo<TKey, TValue> ApplyRegressiveAction(GoapStateInfo<TKey, TValue> stateInfo, out bool reached)
        {
            if (!ProceduralConditions(stateInfo))
            {
                reached = false;
                return null;
            }
            
            var worldState = DoApplyAction(stateInfo);
            var goapGoal = stateInfo.CurrentGoal;

            //Al aplicar un filtro solo se tienen en cuenta los efectos de la acción actual,
            //es decir, efectos anteriores son ignorados aunque si que suman en conjunto si
            //el valor se actualiza.
            var filter = _effects;
            if (_proceduralEffects is not null) filter += _proceduralEffects;
            var remainingGoalConditions = goapGoal.GetFilteredConflicts(worldState, filter);
            worldState.CheckFilteredConflicts(_preconditions, out var newGoalConditions, filter);

            if(remainingGoalConditions == null && newGoalConditions != null) goapGoal = new GoapGoal<TKey, TValue>(goapGoal.Name, newGoalConditions, goapGoal.PriorityLevel);
            else if (remainingGoalConditions != null && newGoalConditions == null) goapGoal = new GoapGoal<TKey, TValue>(goapGoal.Name, remainingGoalConditions, goapGoal.PriorityLevel);
            else if (remainingGoalConditions == null)
            {
                reached = true;
                return new GoapStateInfo<TKey, TValue>(worldState,
                    new GoapGoal<TKey, TValue>("Victory",
                        new PropertyGroup<TKey, TValue>(), 1));
            }
            else if (newGoalConditions.CheckConditionsConflict(remainingGoalConditions))
            {
                reached = false;
                return null;
            }
            else goapGoal = new GoapGoal<TKey, TValue>(goapGoal.Name, remainingGoalConditions + newGoalConditions, goapGoal.PriorityLevel);
            reached = false;
            return new GoapStateInfo<TKey, TValue>(worldState, goapGoal);
        }

        //Used only by the Agent.
        public PropertyGroup<TKey, TValue> Execute(PropertyGroup<TKey, TValue> worldState,
            IGoapAgent<TKey, TValue> goapAgent)
        {
            worldState += _effects;
            if(_proceduralEffects != null) worldState += _proceduralEffects;
            PerformedActions(goapAgent);
            return worldState;
        }

        //Internal methods.
        private bool CheckAction(GoapStateInfo<TKey, TValue> stateInfo)
        {
            if (!stateInfo.WorldState.CheckConflict(_preconditions))
            {
                return ProceduralConditions(stateInfo);
            }
            return false;
        }
        
        private PropertyGroup<TKey, TValue> DoApplyAction(GoapStateInfo<TKey, TValue> stateInfo)
        {
            var worldState = stateInfo.WorldState + _effects;
            _proceduralEffects = GetProceduralEffects(new GoapStateInfo<TKey, TValue>
                (worldState, stateInfo.CurrentGoal));
            if (_proceduralEffects != null) worldState += _proceduralEffects;
            return worldState;
        }

        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}