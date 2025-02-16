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
        private Conditions _preconditions = new();
        private Effects _effects = new();
        private int _cost = 1;
        
        protected IAgent _agent;
        
        public void Initialize(string name, Conditions conditions, Effects effects, IAgent agent)
        {
            Name = name;
            if (conditions != null) _preconditions = conditions;
            if (effects != null) _effects = effects;
            _agent = agent;
            Init();
        }

        protected abstract void Init(); 
        
        //Main abstract
        public virtual bool Validate(State nextState, string[] parameters)
        {
            return OnValidate(nextState, parameters);
        }
        
        public virtual Task<Effects> Execute(Effects effects, string[] parameters, CancellationToken token)
        {
            return OnExecute(effects, parameters, token);
        }

        //Procedural related.
        protected virtual Conditions GetProceduralConditions(ActionSettings settings) => null;
        protected virtual Effects GetProceduralEffects(ActionSettings settings) => null;
        protected virtual bool OnValidate(State state, string[] parameters) => true;
        protected abstract Task<Effects> OnExecute(Effects effects, string[] parameters, CancellationToken token);
        
        //Cost related.
        public int GetCost() => _cost;        
        public virtual int GetCost(Conditions goal) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        public virtual ActionSettings CreateSettings(ActionSettings settings)
        {
            return settings;
        }
        
        public Conditions GetPreconditions(ActionSettings settings)
        {
            return _preconditions + GetProceduralConditions(settings);
        }

        public Effects GetEffects(ActionSettings settings)
        {
            return _effects + GetProceduralEffects(settings);
        }
        public HashSet<PropertyKey> GetAffectedKeys()
        {
            HashSet<PropertyKey> affectedPropertyLists = new HashSet<PropertyKey>();
            foreach (var key in _effects.GetPropertyKeys())
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
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}