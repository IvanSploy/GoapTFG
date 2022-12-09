using System;

namespace GoapTFG.Base
{
    public class Action<TA, TB>
    {
        private string _id;
        private readonly PropertyGroup<TA, TB> _preconditions;
        private readonly PropertyGroup<TA, TB> _effects;

        public event Action PerformedActions;
        public delegate bool Condition(PropertyGroup<TA, TB> pg);
        
        public delegate PropertyGroup<TA, TB> Effect(PropertyGroup<TA, TB> pg);
        public event Condition ProceduralConditions;
        public event Effect ProceduralEffects;

        public Action(string id, PropertyGroup<TA, TB> prePropertyGroup = null, PropertyGroup<TA, TB> effectPropertyGroup = null)
        {
            _id = id;
            _preconditions = prePropertyGroup ?? new PropertyGroup<TA, TB>();
            _effects = effectPropertyGroup ?? new PropertyGroup<TA, TB>();
            PerformedActions += () => Console.Out.WriteLine("Acción ejecutada: " + this);
        }
        
        //GOAP utilities.
        public bool CheckAction(PropertyGroup<TA, TB> propertyGroup)
        {
            if (!propertyGroup.CheckConflict(_preconditions))
            {
                if (ProceduralConditions != null)
                {
                    return ProceduralConditions.Invoke(propertyGroup);
                }
                return true;
            }

            return false;
        }

        public PropertyGroup<TA, TB> ApplyAction(PropertyGroup<TA, TB> propertyGroup)
        {
            Console.Out.WriteLine("Acción aplicada: " + this);
            var result = propertyGroup + _effects;
            var aux = ProceduralEffects?.Invoke(result);
            if (aux != null) result += aux;
            return result;
        }
        
        public PropertyGroup<TA, TB> CheckApplyAction(PropertyGroup<TA, TB> propertyGroup)
        {
            if (!CheckAction(propertyGroup)) return null;
            return ApplyAction(propertyGroup);
        }

        public void PerformAction()
        {
            OnPerformedActions();
        }

        protected virtual void OnPerformedActions()
        {
            PerformedActions?.Invoke();
        }
        
        //Overrides
        public override string ToString()
        {
            return _id + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects;
        }
    }
}