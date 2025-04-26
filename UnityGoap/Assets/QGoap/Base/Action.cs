using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static LUGoap.Base.PropertyManager;

namespace LUGoap.Base
{
    [Serializable]
    public abstract class Action
    {
        //Fields
        public string Name { get; private set; }
        private ConditionGroup _preconditions = new();
        private EffectGroup _effectGroup = new();
        private int _cost = 1;
        
        protected IAgent _agent;
        
        public void Initialize(string name, ConditionGroup conditionGroup, EffectGroup effectGroup, IAgent agent)
        {
            Name = name;
            if (conditionGroup != null) _preconditions = conditionGroup;
            if (effectGroup != null) _effectGroup = effectGroup;
            _agent = agent;
            Init();
        }

        protected abstract void Init(); 
        
        //Main abstract
        public virtual bool Validate(State nextState, string[] parameters)
        {
            return OnValidate(nextState, parameters);
        }
        
        public virtual Task<EffectGroup> Execute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
        {
            return OnExecute(effectGroup, parameters, token);
        }

        //Procedural related.
        protected virtual ConditionGroup GetProceduralConditions(ActionSettings settings) => null;
        protected virtual EffectGroup GetProceduralEffects(ActionSettings settings) => null;
        protected virtual bool OnValidate(State state, string[] parameters) => true;
        protected abstract Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token);
        
        //Cost related.
        public int GetCost() => _cost;        
        public virtual int GetCost(ConditionGroup goal) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        public virtual ActionSettings CreateSettings(ActionSettings settings)
        {
            return settings;
        }
        
        public ConditionGroup GetPreconditions(ActionSettings settings)
        {
            return _preconditions.Combine(GetProceduralConditions(settings), true);
        }

        public EffectGroup GetEffects(ActionSettings settings)
        {
            return _effectGroup + GetProceduralEffects(settings);
        }
        public HashSet<PropertyKey> GetAffectedKeys()
        {
            HashSet<PropertyKey> affectedPropertyLists = new HashSet<PropertyKey>();
            foreach (var key in _effectGroup.GetPropertyKeys())
            {
                affectedPropertyLists.Add(key);
            }

            var proceduralEffects = GetProceduralEffects(ActionSettings.CreateDefault(this));

            if (proceduralEffects != null)
            {
                foreach (var key in proceduralEffects.GetPropertyKeys())
                {
                    affectedPropertyLists.Add(key);
                }
            }
            return affectedPropertyLists;
        }

        //Overrides
        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effectGroup
                   ;
        }
    }
}