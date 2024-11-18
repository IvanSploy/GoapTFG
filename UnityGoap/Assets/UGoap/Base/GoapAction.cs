using System.Collections.Generic;
using System.Threading.Tasks;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Base
{
    public abstract class GoapAction
    {
        //Fields
        public string Name { get; }
        private readonly GoapConditions _preconditions = new();
        private readonly GoapEffects _effects = new();
        private int _cost = 1;

        protected GoapAction() { }
        
        protected GoapAction(string name, GoapConditions conditions, GoapEffects effects)
        {
            Name = name;
            if (conditions != null) _preconditions = conditions;
            if (effects != null) _effects = effects;
        }

        //Procedural related.
        protected abstract GoapConditions GetProceduralConditions(GoapSettings settings);
        protected abstract GoapEffects GetProceduralEffects(GoapSettings settings);
        public abstract bool Validate(GoapState state, GoapActionInfo actionInfo, IGoapAgent agent);
        public abstract Task<GoapState> Execute(GoapState state, IGoapAgent agent);
        
        //Cost related.
        public int GetCost() => _cost;        
        public virtual int GetCost(GoapConditions goal) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public GoapConditions GetPreconditions(GoapSettings settings)
        {
            return _preconditions + GetProceduralConditions(settings);
        }

        public GoapEffects GetEffects(GoapSettings settings)
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

            var proceduralEffects = GetProceduralEffects(GoapSettings.GetDefault());

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