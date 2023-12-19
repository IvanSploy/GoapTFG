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
        private HashSet<TKey> _affectedKeys;
        private int _cost = 1;
        public bool IsCompleted { get; } = false;
        private PropertyGroup<TKey, TValue> _proceduralEffects;

        //Creation of the scriptable TValue
        protected GoapAction(PropertyGroup<TKey, TValue> preconditions,
            PropertyGroup<TKey, TValue> effects,
            HashSet<TKey> affectedKeys = null)
        {
            _preconditions = preconditions;
            _effects = effects;
            _affectedKeys = InitializeAffectedKeys(affectedKeys);
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
        public HashSet<TKey> GetAffectedKeys() => _affectedKeys;
        
        private HashSet<TKey> InitializeAffectedKeys(HashSet<TKey> affectedKeys = null)
        {
            HashSet<TKey> affectedPropertyLists = new HashSet<TKey>();
            affectedPropertyLists.AddRange(_effects.GetKeys());
            if(affectedKeys != null) affectedPropertyLists.AddRange(affectedKeys);
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
            var goal = stateInfo.CurrentGoal;

            //Al aplicar un filtro solo se tienen en cuenta los efectos de la acción actual,
            //es decir, efectos anteriores son ignorados aunque si que suman en conjunto si
            //el valor se actualiza.
            var filter = _effects;
            if (_proceduralEffects is not null) filter += _proceduralEffects;
            var remainingGoalConditions = goal.ResolveFilteredGoal(worldState, filter);
            worldState.CheckFilteredConflict(_preconditions, out var newGoalConditions, filter);

            if(remainingGoalConditions == null && newGoalConditions != null) goal = new GoapGoal<TKey, TValue>(goal.Name, newGoalConditions, goal.PriorityLevel);
            else if (remainingGoalConditions != null && newGoalConditions == null) goal = new GoapGoal<TKey, TValue>(goal.Name, remainingGoalConditions, goal.PriorityLevel);
            else if (remainingGoalConditions == null)
            {
                reached = true;
                return GetVictoryGoal(worldState);
            }
            else if (newGoalConditions.CheckConditionsConflict(remainingGoalConditions))
            {
                reached = false;
                return null;
            }
            else goal = new GoapGoal<TKey, TValue>(goal.Name, remainingGoalConditions + newGoalConditions, goal.PriorityLevel);
            
            reached = false;
            return new GoapStateInfo<TKey, TValue>(worldState, goal);
        }
        
        public (PropertyGroup<TKey, TValue>, GoapGoal<TKey, TValue>, bool) ApplyMixedAction(PropertyGroup<TKey, TValue> state, GoapGoal<TKey, TValue> goal)
        {
            PropertyGroup<TKey, TValue> resultState;
            GoapGoal<TKey, TValue> resultGoal;
            bool proceduralCheck = ProceduralConditions(new GoapStateInfo<TKey, TValue>(state, goal));
            
            resultState = DoApplyAction(new GoapStateInfo<TKey, TValue>(state, goal));
            var conflicts = resultState.GetConflict(_preconditions);
            resultGoal = new GoapGoal<TKey, TValue>(goal.Name, conflicts, goal.PriorityLevel);
            return (resultState, resultGoal, proceduralCheck);
        }
        
        private GoapStateInfo<TKey, TValue> GetVictoryGoal(PropertyGroup<TKey, TValue> worldState)
        {
            return new GoapStateInfo<TKey, TValue>(worldState,
                new GoapGoal<TKey, TValue>("Victory",
                    new PropertyGroup<TKey, TValue>(), 1));
        }

        //Used only by the Agent.
        public PropertyGroup<TKey, TValue> Execute(GoapStateInfo<TKey, TValue> stateInfo,
            IGoapAgent<TKey, TValue> goapAgent)
        {
            if (!CheckAction(stateInfo)) return null;
            var state = stateInfo.WorldState + _effects;
            if(_proceduralEffects != null) state += _proceduralEffects;
            PerformedActions(goapAgent);
            return state;
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