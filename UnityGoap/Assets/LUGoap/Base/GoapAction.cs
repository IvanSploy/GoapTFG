using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static LUGoap.Base.PropertyManager;

namespace LUGoap.Base
{
    public abstract class Action
    {
        //Fields
        public string Name { get; private set; }
        private Conditions _preconditions = new();
        private Effects _effects = new();
        private int _cost = 1;
        
        public void Initialize(string name, Conditions conditions, Effects effects)
        {
            Name = name;
            if (conditions != null) _preconditions = conditions;
            if (effects != null) _effects = effects;
        }
        
        //Main abstract
        public virtual string[] CreateParameters(int learningCode) => null;

        public virtual bool Validate(State nextState, IAgent iAgent, string[] parameters)
        {
            return OnValidate(nextState, iAgent, parameters);
        }
        
        public virtual Task<Effects> Execute(Effects effects, IAgent iAgent, string[] parameters, CancellationToken token)
        {
            var result = OnExecute(effects, iAgent, parameters, token);
            if (token.IsCancellationRequested) return null;

            return result;
        }

        //Procedural related.
        protected abstract Conditions GetProceduralConditions(ActionSettings settings);
        protected abstract Effects GetProceduralEffects(ActionSettings settings);
        protected abstract bool OnValidate(State state, IAgent iAgent, string[] parameters);
        protected abstract Task<Effects> OnExecute(Effects effects, IAgent iAgent, string[] parameters, CancellationToken token);
        
        //Cost related.
        public int GetCost() => _cost;        
        public virtual int GetCost(Conditions goal) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
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