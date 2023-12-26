using System.Collections.Generic;

namespace UGoap.Base
{
    public abstract class GoapAction<TKey, TValue> : IGoapAction<TKey, TValue>
    {
        //Fields
        public string Name { get; private set; }
        private ConditionGroup<TKey, TValue> _preconditions;
        private EffectGroup<TKey, TValue> _effects;
        private HashSet<TKey> _affectedKeys;
        private int _cost = 1;
        public bool IsCompleted { get; } = false;

        //Creation of the scriptable TValue
        protected GoapAction(ConditionGroup<TKey, TValue> preconditions,
            EffectGroup<TKey, TValue> effects,
            HashSet<TKey> affectedKeys = null)
        {
            _preconditions = preconditions;
            _effects = effects;
            _affectedKeys = InitializeAffectedKeys(affectedKeys);
        }

        //Procedural related.
        protected abstract bool ProceduralConditions(GoapStateInfo<TKey, TValue> stateInfo);
        protected abstract EffectGroup<TKey, TValue> GetProceduralEffects(GoapStateInfo<TKey, TValue> stateInfo);
        protected abstract void PerformedActions(IGoapAgent<TKey, TValue> agent);
        
        //Cost related.
        public int GetCost() => _cost;
        public abstract IGoapAction<TKey, TValue> Clone();

        public virtual int GetCost(GoapStateInfo<TKey, TValue> stateInfo) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public ConditionGroup<TKey, TValue> GetPreconditions() => _preconditions;
        public EffectGroup<TKey, TValue> GetEffects() => _effects;
        public HashSet<TKey> GetAffectedKeys() => _affectedKeys;
        
        private HashSet<TKey> InitializeAffectedKeys(HashSet<TKey> affectedKeys = null)
        {
            HashSet<TKey> affectedPropertyLists = new HashSet<TKey>();
            foreach (var key in _effects.GetKeys())
            {
                affectedPropertyLists.Add(key);
            }

            if (affectedKeys != null)
            {
                foreach (var key in affectedKeys)
                {
                    affectedPropertyLists.Add(key);
                }
            }
            return affectedPropertyLists;
        }

        //GOAP utilities.
        public (PropertyGroup<TKey, TValue> state, EffectGroup<TKey, TValue> proceduralEffects) ApplyAction(GoapStateInfo<TKey, TValue> stateInfo)
        {
            if (!CheckAction(stateInfo)) return (null, null);
            return DoApplyAction(stateInfo);
        }

        public GoapStateInfo<TKey, TValue> ApplyRegressiveAction(GoapStateInfo<TKey, TValue> stateInfo, out bool reached)
        {
            if (!ProceduralConditions(stateInfo))
            {
                reached = false;
                return null;
            }
            
            (var worldState, var proceduralEffects) =
                DoApplyAction(stateInfo);
            var goal = stateInfo.Goal;

            //Filtro de objetivos (solo los que atañen a la accion actual.)
            PropertyGroup<TKey, TValue> filter = _effects;
            if (proceduralEffects is not null) filter += proceduralEffects;
            
            var remainingGoalConditions = goal.ResolveFilteredGoal(worldState, filter);
            _preconditions.CheckFilteredConflict(worldState, out var newGoalConditions, filter);
            
            if (remainingGoalConditions == null && newGoalConditions == null)
            {
                reached = true;
                return new GoapStateInfo<TKey, TValue>(worldState, GetVictoryGoal(), proceduralEffects);
            }

            ConditionGroup<TKey, TValue> conditionGroup = ConditionGroup<TKey, TValue>.Merge(remainingGoalConditions, newGoalConditions);
            if (conditionGroup == null)
            {
                reached = false;
                return null;
            }
            
            goal = new GoapGoal<TKey, TValue>(goal.Name, conditionGroup, goal.PriorityLevel);

            reached = false;
            return new GoapStateInfo<TKey, TValue>(worldState, goal, proceduralEffects);
        }
        
        public (GoapStateInfo<TKey, TValue>, bool) ApplyMixedAction(PropertyGroup<TKey, TValue> state, GoapGoal<TKey, TValue> goal)
        {
            //Check conflicts
            var conflicts = _preconditions.GetConflict(state);
            bool proceduralCheck = ProceduralConditions(new GoapStateInfo<TKey, TValue>(state, goal));
            
            //Apply action
            (var resultState, var proceduralEffects) =
                DoApplyAction(new GoapStateInfo<TKey, TValue>(state, goal));
            var resultGoal = conflicts == null ? GetVictoryGoal() : new GoapGoal<TKey, TValue>(goal.Name, conflicts, goal.PriorityLevel);
            return (new GoapStateInfo<TKey, TValue>(resultState, resultGoal, proceduralEffects), proceduralCheck);
        }
        
        private GoapGoal<TKey, TValue> GetVictoryGoal()
        {
            return new GoapGoal<TKey, TValue>("Victory", new ConditionGroup<TKey, TValue>(), 1);
        }

        //Used only by the Agent.
        public PropertyGroup<TKey, TValue> Execute(GoapStateInfo<TKey, TValue> stateInfo,
            IGoapAgent<TKey, TValue> goapAgent)
        {
            if (!CheckAction(stateInfo)) return null;
            var state = stateInfo.State + _effects;
            if(stateInfo.ProceduralEffects != null) state += stateInfo.ProceduralEffects;
            PerformedActions(goapAgent);
            return state;
        }

        //Internal methods.
        private bool CheckAction(GoapStateInfo<TKey, TValue> stateInfo)
        {
            if (!_preconditions.CheckConflict(stateInfo.State))
            {
                return ProceduralConditions(stateInfo);
            }
            return false;
        }
        
        private (PropertyGroup<TKey, TValue> state, EffectGroup<TKey, TValue> proceduralEffects) DoApplyAction(GoapStateInfo<TKey, TValue> stateInfo)
        {
            var worldState = stateInfo.State + _effects;
            var proceduralEffects = GetProceduralEffects(new GoapStateInfo<TKey, TValue>
                (worldState, stateInfo.Goal));
            if (proceduralEffects != null) worldState += proceduralEffects;
            return (worldState, proceduralEffects);
        }

        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}