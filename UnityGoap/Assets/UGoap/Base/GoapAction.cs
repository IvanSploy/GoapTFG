using System.Collections.Generic;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Base
{
    public abstract class GoapAction : IGoapAction
    {
        //Fields
        public string Name { get; private set; }
        private GoapConditions _preconditions;
        private GoapEffects _effects;
        private HashSet<PropertyKey> _affectedKeys;
        private int _cost = 1;
        public bool IsCompleted { get; } = false;

        //Creation of the scriptable TValue
        protected GoapAction(GoapConditions preconditions,
            GoapEffects effects,
            HashSet<PropertyKey> affectedKeys = null)
        {
            _preconditions = preconditions;
            _effects = effects;
            _affectedKeys = InitializeAffectedKeys(affectedKeys);
        }

        //Procedural related.
        protected abstract bool Validate(GoapStateInfo stateInfo);
        protected abstract GoapConditions GetProceduralConditions(GoapStateInfo stateInfo);
        protected abstract GoapEffects GetProceduralEffects(GoapStateInfo stateInfo);
        protected abstract void PerformedActions(GoapState goapState, IGoapAgent agent);
        
        //Cost related.
        public int GetCost() => _cost;
        public abstract IGoapAction Clone();

        public virtual int GetCost(GoapState state, GoapGoal goal) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public GoapConditions GetPreconditions(GoapStateInfo stateInfo)
        {
            return _preconditions + GetProceduralConditions(stateInfo);
        }

        public GoapEffects GetEffects(GoapStateInfo stateInfo)
        {
            
            return _effects + GetProceduralEffects(stateInfo);
        }

        public HashSet<PropertyKey> GetAffectedKeys() => _affectedKeys;
        
        private HashSet<PropertyKey> InitializeAffectedKeys(HashSet<PropertyKey> affectedKeys = null)
        {
            HashSet<PropertyKey> affectedPropertyLists = new HashSet<PropertyKey>();
            foreach (var key in _effects.GetPropertyKeys())
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
        
        public (GoapState State, GoapGoal Goal) ApplyAction(GoapStateInfo stateInfo)
        {
            //Check conflict
            if(!Validate(stateInfo)) return (null, null);
            
            var conflicts = GetPreconditions(stateInfo).GetConflict(stateInfo.State);
            
            //Apply action
            var resultState = DoApplyAction(stateInfo);
            var resultGoal = conflicts == null ? GetVictoryGoal() : new GoapGoal(stateInfo.Goal.Name, conflicts, stateInfo.Goal.PriorityLevel);
            return (resultState, resultGoal);
        }
        
        private GoapGoal GetVictoryGoal()
        {
            return new GoapGoal("Victory", new GoapConditions(), 1);
        }

        //Used only by the Agent.
        public GoapState Execute(GoapStateInfo stateInfo,
            IGoapAgent goapAgent)
        {
            if (!CheckAction(stateInfo)) return null;
            var state = stateInfo.State + GetEffects(stateInfo);
            PerformedActions(state, goapAgent);
            return state;
        }

        //Internal methods.
        private bool CheckAction(GoapStateInfo stateInfo)
        {
            if (!GetPreconditions(stateInfo).CheckConflict(stateInfo.State))
            {
                return Validate(stateInfo);
            }
            return false;
        }
        
        private GoapState DoApplyAction(GoapStateInfo stateInfo)
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