using System;

namespace GoapTFG.Base
{
    public class Action<TA, TB>
    {
        public int Cost = 1;
        private string _id;
        private readonly PropertyGroup<TA, TB> _preconditions;
        private readonly PropertyGroup<TA, TB> _effects;

        public event Action PerformedActions;
        public delegate bool Condition(PropertyGroup<TA, TB> pg);
        
        public delegate void Effect();
        public event Condition ProceduralConditions;
        public event Effect ProceduralEffects;

        public Action(string id, PropertyGroup<TA, TB> preconditions = null, PropertyGroup<TA, TB> effects = null)
        {
            _id = id;
            _preconditions = preconditions ?? new PropertyGroup<TA, TB>();
            _effects = effects ?? new PropertyGroup<TA, TB>();
            PerformedActions += () => Console.Out.WriteLine(this);
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
            return worldState + _effects;
        }
        
        public PropertyGroup<TA, TB> ForceAction(PropertyGroup<TA, TB> worldState)
        {
            return worldState + _effects;
        }

        public PropertyGroup<TA, TB> PerformAction(PropertyGroup<TA, TB> worldState)
        {
            var state = ApplyAction(worldState);
            PerformedActions?.Invoke();
            return state;
        }
        
        //Overrides
        public override string ToString()
        {
            return _id 
                   //+ " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}