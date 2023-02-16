using System;

namespace GoapTFG.Base
{
    public class Action<TA, TB>
    {
        public IAgent<TA, TB> Agent;
        public string Name;

        public int Cost
        {
            set => _cost = value;
        }

        private readonly PropertyGroup<TA, TB> _preconditions;
        private readonly PropertyGroup<TA, TB> _effects;
        private int _cost = 1;
        private Func<IAgent<TA, TB>, PropertyGroup<TA, TB>, int> _customCost;
        
        public delegate bool Condition(IAgent<TA, TB> agent, PropertyGroup<TA, TB> worldState);
        public delegate void Effect(IAgent<TA, TB> agent, PropertyGroup<TA, TB> worldState);
        public event Condition ProceduralConditions;
        public event Effect ProceduralEffects;
        public event Effect PerformedActions;

        public Action(IAgent<TA, TB> agent, string name, PropertyGroup<TA, TB> preconditions = null, PropertyGroup<TA, TB> effects = null)
        {
            Agent = agent;
            Name = name;
            _preconditions = preconditions != null ?
                new PropertyGroup<TA, TB>(preconditions) : new PropertyGroup<TA, TB>();
            _effects = effects != null ? new PropertyGroup<TA, TB>(effects) : new PropertyGroup<TA, TB>();
        }

        //Cost related.
        public void SetCustomCost(Func<IAgent<TA, TB>, PropertyGroup<TA, TB>, int> customCost)
        {
            _customCost = customCost;
        }

        public int GetCost()
        {
            return _cost;
        }
        
        public int GetCost(PropertyGroup<TA, TB> currentState)
        {
            return _customCost?.Invoke(Agent, currentState) ?? _cost;
        }
        
        //GOAP utilities.
        public bool CheckAction(PropertyGroup<TA, TB> worldState)
        {
            if (!worldState.CheckConflict(_preconditions))
            {
                if (ProceduralConditions != null)
                {
                    return ProceduralConditions.Invoke(Agent, worldState);
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
                ProceduralEffects.Invoke(Agent, worldState);
            }
            return worldState;
        }
        
        public PropertyGroup<TA, TB> ApplyRegresiveAction(PropertyGroup<TA, TB> worldState, ref Goal<TA, TB> goal, out bool reached)
        {
            worldState = ForceAction(worldState);
            Goal<TA, TB> auxGoal = goal + _preconditions;
            //auxGoal.ProceduralConditions += ProceduralConditions;
            var conflicts = auxGoal.GetConflicts(worldState);
            if (conflicts != null)
            {
                goal = new Goal<TA, TB>(goal.Name, conflicts, goal.PriorityLevel);
                reached = false;
            }
            //reached = goal.CheckProcedural(Agent, worldState);
            reached = true;
            return worldState;
        }
        
        public PropertyGroup<TA, TB> ForceAction(PropertyGroup<TA, TB> worldState)
        {
            worldState += _effects;
            if (ProceduralEffects != null)
            {
                ProceduralEffects.Invoke(Agent, worldState);
            }

            return worldState;
        }

        public PropertyGroup<TA, TB> PerformAction(PropertyGroup<TA, TB> worldState)
        {
            worldState = ApplyAction(worldState);
            PerformedActions?.Invoke(Agent, worldState);
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