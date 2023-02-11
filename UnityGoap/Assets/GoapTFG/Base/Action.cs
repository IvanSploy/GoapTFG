using System;
using GoapTFG.Planner;

namespace GoapTFG.Base
{
    public class Action<TA, TB>
    {
        public IAgent<TA, TB> Agent;
        public int Cost = 1;
        public string Name;
        private readonly PropertyGroup<TA, TB> _preconditions;
        private readonly PropertyGroup<TA, TB> _effects;

        public delegate bool Condition(PropertyGroup<TA, TB> worldState);
        public delegate void Effect(PropertyGroup<TA, TB> worldState);
        
        public delegate void PerformedAction(IAgent<TA, TB> agent);
        public event Condition ProceduralConditions;
        public event Effect ProceduralEffects;
        public event PerformedAction PerformedActions;

        public Action(IAgent<TA, TB> agent, string name, PropertyGroup<TA, TB> preconditions = null, PropertyGroup<TA, TB> effects = null)
        {
            this.Agent = agent;
            Name = name;
            _preconditions = preconditions != null ?
                new PropertyGroup<TA, TB>(preconditions) : new PropertyGroup<TA, TB>();
            _effects = effects != null ? new PropertyGroup<TA, TB>(effects) : new PropertyGroup<TA, TB>();
        }
        
        //GOAP utilities.
        public bool CheckAction(PropertyGroup<TA, TB> worldState)
        {
            if (!worldState.CheckConflict(_preconditions))
            {
                if (ProceduralConditions != null)
                {
                    return ProceduralConditions.Invoke(worldState);
                }
                return true;
            }
            return false;
        }

        public PropertyGroup<TA, TB> ApplyAction(PropertyGroup<TA, TB> worldState)
        {
            //Console.Out.WriteLine("Acción aplicada: " + this);
            if (!CheckAction(worldState)) return null;
            worldState += _effects;
            if (ProceduralEffects != null)
            {
                ProceduralEffects.Invoke(worldState);
            }
            return worldState;
        }
        
        public PropertyGroup<TA, TB> ForceAction(PropertyGroup<TA, TB> worldState)
        {
            return worldState + _effects;
        }

        public PropertyGroup<TA, TB> PerformAction(PropertyGroup<TA, TB> worldState)
        {
            worldState = ApplyAction(worldState);
            PerformedActions?.Invoke(Agent);
            return worldState;
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