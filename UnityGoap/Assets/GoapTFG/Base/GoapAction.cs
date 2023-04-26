using System;
using System.Net;

namespace GoapTFG.Base
{
    public class GoapAction<TA, TB>
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

        public GoapAction(IAgent<TA, TB> agent, string name, PropertyGroup<TA, TB> preconditions = null, 
            PropertyGroup<TA, TB> effects = null)
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
        
        //Getters
        public PropertyGroup<TA, TB> GetPreconditions()
        {
            return _preconditions;
        }
        
        public PropertyGroup<TA, TB> GetEffects()
        {
            return _effects;
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
        
        public PropertyGroup<TA, TB> ApplyRegressiveAction(PropertyGroup<TA, TB> worldState, ref GoapGoal<TA, TB> goapGoal, out bool reached)
        {
            if (ProceduralConditions != null)
            {
                if (!ProceduralConditions.Invoke(Agent, worldState))
                {
                    reached = false;
                    return null;
                }
            }
            var ws = ForceAction(worldState);
            var firstState = goapGoal.GetConflicts(ws);
            ws.CheckConflict(_preconditions, out var lastState);
            if(firstState == null && lastState != null) goapGoal = new GoapGoal<TA, TB>(goapGoal.Name, lastState, goapGoal.PriorityLevel);
            else if (firstState != null && lastState == null) goapGoal = new GoapGoal<TA, TB>(goapGoal.Name, firstState, goapGoal.PriorityLevel);
            else if (firstState == null)
            {
                reached = true;
                return ws;
            }
            else if (lastState.CheckConditionsConflict(firstState))
            {
                reached = false;
                return null;
            }
            else goapGoal = new GoapGoal<TA, TB>(goapGoal.Name, firstState + lastState, goapGoal.PriorityLevel);
            reached = false;
            return ws;
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