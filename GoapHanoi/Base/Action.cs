using System;

namespace GoapHanoi.Base
{
    public class Action<TA, TB>
    {
        private string _id;
        private readonly PropertyGroup<TA, TB> _preconditions;
        private readonly PropertyGroup<TA, TB> _effects;

        public event Action PerformedActions;

        public Action(string id, PropertyGroup<TA, TB> prePropertyGroup = null, PropertyGroup<TA, TB> effectPropertyGroup = null)
        {
            _id = id;
            _preconditions = prePropertyGroup ?? new PropertyGroup<TA, TB>();
            _effects = effectPropertyGroup ?? new PropertyGroup<TA, TB>();
            PerformedActions += () => System.Console.Out.WriteLine("Acción ejecutada: " + this);
        }
        
        //GOAP utilities.
        public bool CheckAction(PropertyGroup<TA, TB> propertyGroup)
        {
            return propertyGroup.CheckConflict(_preconditions);
        }

        public PropertyGroup<TA, TB> ApplyAction(PropertyGroup<TA, TB> propertyGroup)
        {
            OnPerformedActions();
            return propertyGroup + _effects;
        }
        
        public PropertyGroup<TA, TB> CheckApplyAction(PropertyGroup<TA, TB> propertyGroup)
        {
            if (CheckAction(propertyGroup)) return null;
            return ApplyAction(propertyGroup);
        }

        protected virtual void OnPerformedActions()
        {
            PerformedActions?.Invoke();
        }
        
        //Overrides
        public override string ToString()
        {
            return _id + " ->\n" + _effects;
        }
    }
}