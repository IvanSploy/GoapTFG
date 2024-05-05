using System.Collections.Generic;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Base
{
    public abstract class GoapAction : IGoapAction
    {
        //Fields
        public string Name { get; private set; }
        protected readonly GoapConditions _preconditions = new();
        protected readonly GoapEffects _effects = new();
        private int _cost = 1;

        //Updating data from the scriptable object.
        protected GoapAction(string name, GoapConditions conditions, GoapEffects effects)
        {
            Name = name;
            if (conditions != null) _preconditions = conditions;
            if (effects != null) _effects = effects;
        }

        //Procedural related.
        protected abstract GoapConditions GetProceduralConditions(GoapConditions goal);
        protected abstract GoapEffects GetProceduralEffects(GoapConditions goal);
        public abstract bool Validate(GoapState state, IGoapAgent agent);
        public abstract void Execute(GoapState state, IGoapAgent agent);
        
        //Cost related.
        public int GetCost() => _cost;        
        public virtual int GetCost(GoapConditions goal) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public GoapConditions GetPreconditions(GoapConditions goal)
        {
            return _preconditions + GetProceduralConditions(goal);
        }

        public GoapEffects GetEffects(GoapConditions goal)
        {
            return _effects + GetProceduralEffects(goal);
        }
        public HashSet<PropertyKey> GetAffectedKeys()
        {
            HashSet<PropertyKey> affectedPropertyLists = new HashSet<PropertyKey>();
            foreach (var key in _effects.GetPropertyKeys())
            {
                affectedPropertyLists.Add(key);
            }

            var proceduralEffects = GetProceduralEffects(new GoapConditions());

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