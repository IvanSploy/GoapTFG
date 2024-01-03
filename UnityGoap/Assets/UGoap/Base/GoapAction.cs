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
        protected abstract bool Validate(GoapStateInfo<TKey, TValue> stateInfo);
        protected abstract ConditionGroup<TKey, TValue> GetProceduralConditions(GoapStateInfo<TKey, TValue> stateInfo);
        protected abstract EffectGroup<TKey, TValue> GetProceduralEffects(GoapStateInfo<TKey, TValue> stateInfo);
        protected abstract void PerformedActions(StateGroup<TKey, TValue> state, IGoapAgent<TKey, TValue> agent);
        
        //Cost related.
        public int GetCost() => _cost;
        public abstract IGoapAction<TKey, TValue> Clone();

        public virtual int GetCost(GoapStateInfo<TKey, TValue> stateInfo) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public ConditionGroup<TKey, TValue> GetPreconditions(GoapStateInfo<TKey, TValue> stateInfo)
        {
            return _preconditions + GetProceduralConditions(stateInfo);
        }

        public EffectGroup<TKey, TValue> GetEffects(GoapStateInfo<TKey, TValue> stateInfo)
        {
            
            return _effects + GetProceduralEffects(stateInfo);
        }

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
        public StateGroup<TKey, TValue> ApplyAction(GoapStateInfo<TKey, TValue> stateInfo)
        {
            if (!CheckAction(stateInfo)) return null;
            return DoApplyAction(stateInfo);
        }

        public GoapStateInfo<TKey, TValue> ApplyRegressiveAction(GoapStateInfo<TKey, TValue> stateInfo, out bool reached)
        {
            if (!Validate(stateInfo))
            {
                reached = false;
                return null;
            }
            
            var worldState = DoApplyAction(stateInfo);
            var goal = stateInfo.Goal;

            //Filtro de objetivos (solo los que atañen a la accion actual.)
            StateGroup<TKey, TValue> filter = GetEffects(stateInfo);
            
            var remainingGoalConditions = goal.ResolveFilteredGoal(worldState, filter);
            GetPreconditions(stateInfo).CheckFilteredConflict(worldState, out var newGoalConditions, filter);
            
            if (remainingGoalConditions == null && newGoalConditions == null)
            {
                reached = true;
                return new GoapStateInfo<TKey, TValue>(worldState, GetVictoryGoal());
            }

            ConditionGroup<TKey, TValue> conditionGroup = ConditionGroup<TKey, TValue>.Merge(remainingGoalConditions, newGoalConditions);
            if (conditionGroup == null)
            {
                reached = false;
                return null;
            }
            
            goal = new GoapGoal<TKey, TValue>(goal.Name, conditionGroup, goal.PriorityLevel);

            reached = false;
            return new GoapStateInfo<TKey, TValue>(worldState, goal);
        }
        
        public GoapStateInfo<TKey, TValue> ApplyMixedAction(StateGroup<TKey, TValue> state, GoapGoal<TKey, TValue> goal)
        {
            //Check conflicts
            var stateInfo = new GoapStateInfo<TKey, TValue>(state, goal);
            if(!Validate(stateInfo)) return null;
            
            var conflicts = GetPreconditions(stateInfo).GetConflict(state);
            
            //Apply action
            var resultState = DoApplyAction(stateInfo);
            var resultGoal = conflicts == null ? GetVictoryGoal() : new GoapGoal<TKey, TValue>(goal.Name, conflicts, goal.PriorityLevel);
            return new GoapStateInfo<TKey, TValue>(resultState, resultGoal);
        }
        
        private GoapGoal<TKey, TValue> GetVictoryGoal()
        {
            return new GoapGoal<TKey, TValue>("Victory", new ConditionGroup<TKey, TValue>(), 1);
        }

        //Used only by the Agent.
        public StateGroup<TKey, TValue> Execute(GoapStateInfo<TKey, TValue> stateInfo,
            IGoapAgent<TKey, TValue> goapAgent)
        {
            if (!CheckAction(stateInfo)) return null;
            var state = stateInfo.State + GetEffects(stateInfo);
            PerformedActions(state, goapAgent);
            return state;
        }

        //Internal methods.
        private bool CheckAction(GoapStateInfo<TKey, TValue> stateInfo)
        {
            if (!GetPreconditions(stateInfo).CheckConflict(stateInfo.State))
            {
                return Validate(stateInfo);
            }
            return false;
        }
        
        private StateGroup<TKey, TValue> DoApplyAction(GoapStateInfo<TKey, TValue> stateInfo)
        {
            return stateInfo.State + GetEffects(stateInfo);
        }

        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}