using System.Collections.Generic;

namespace UGoap.Base
{
    public abstract class GoapAction<TKey, TValue> : IGoapAction<TKey, TValue>
    {
        //Fields
        public string Name { get; private set; }
        private GoapConditions<TKey, TValue> _preconditions;
        private GoapEffects<TKey, TValue> _effects;
        private HashSet<TKey> _affectedKeys;
        private int _cost = 1;
        public bool IsCompleted { get; } = false;

        //Creation of the scriptable TValue
        protected GoapAction(GoapConditions<TKey, TValue> preconditions,
            GoapEffects<TKey, TValue> effects,
            HashSet<TKey> affectedKeys = null)
        {
            _preconditions = preconditions;
            _effects = effects;
            _affectedKeys = InitializeAffectedKeys(affectedKeys);
        }

        //Procedural related.
        protected abstract bool Validate(GoapStateInfo<TKey, TValue> stateInfo);
        protected abstract GoapConditions<TKey, TValue> GetProceduralConditions(GoapStateInfo<TKey, TValue> stateInfo);
        protected abstract GoapEffects<TKey, TValue> GetProceduralEffects(GoapStateInfo<TKey, TValue> stateInfo);
        protected abstract void PerformedActions(GoapState<TKey, TValue> goapState, IGoapAgent<TKey, TValue> agent);
        
        //Cost related.
        public int GetCost() => _cost;
        public abstract IGoapAction<TKey, TValue> Clone();

        public virtual int GetCost(GoapState<TKey, TValue> state, GoapGoal<TKey,TValue> goal) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public GoapConditions<TKey, TValue> GetPreconditions(GoapStateInfo<TKey, TValue> stateInfo)
        {
            return _preconditions + GetProceduralConditions(stateInfo);
        }

        public GoapEffects<TKey, TValue> GetEffects(GoapStateInfo<TKey, TValue> stateInfo)
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
        
        public (GoapState<TKey, TValue> State, GoapGoal<TKey,TValue> Goal) ApplyAction(GoapStateInfo<TKey, TValue> stateInfo)
        {
            //Check conflict
            if(!Validate(stateInfo)) return (null, null);
            
            var conflicts = GetPreconditions(stateInfo).GetConflict(stateInfo.State);
            
            //Apply action
            var resultState = DoApplyAction(stateInfo);
            var resultGoal = conflicts == null ? GetVictoryGoal() : new GoapGoal<TKey, TValue>(stateInfo.Goal.Name, conflicts, stateInfo.Goal.PriorityLevel);
            return (resultState, resultGoal);
        }
        
        private GoapGoal<TKey, TValue> GetVictoryGoal()
        {
            return new GoapGoal<TKey, TValue>("Victory", new GoapConditions<TKey, TValue>(), 1);
        }

        //Used only by the Agent.
        public GoapState<TKey, TValue> Execute(GoapStateInfo<TKey, TValue> stateInfo,
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
        
        private GoapState<TKey, TValue> DoApplyAction(GoapStateInfo<TKey, TValue> stateInfo)
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