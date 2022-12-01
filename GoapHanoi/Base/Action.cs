using System;

namespace GoapHanoi.Base
{
    public class Action<TA, TB>
    {
        private string _id;
        private readonly PropertyGroup<TA, TB> _preconditionPropertyGroup;
        private readonly PropertyGroup<TA, TB> _effectPropertyGroup;

        public event Action PerformedActions;

        public Action(string id, PropertyGroup<TA, TB> prePropertyGroup = null, PropertyGroup<TA, TB> effectPropertyGroup = null)
        {
            _id = id;
            _preconditionPropertyGroup = prePropertyGroup ?? new PropertyGroup<TA, TB>();
            _effectPropertyGroup = effectPropertyGroup ?? new PropertyGroup<TA, TB>();
            PerformedActions += () => System.Console.Out.WriteLine("Acción ejecutada: " + this);
        }
        
        //GOAP utilities.
        public bool CheckAction(PropertyGroup<TA, TB> propertyGroup)
        {
            return propertyGroup.CheckConflict(_preconditionPropertyGroup);
        }

        public PropertyGroup<TA, TB> ApplyAction(PropertyGroup<TA, TB> propertyGroup)
        {
            OnPerformedActions();
            return propertyGroup + _effectPropertyGroup;
        }
        
        public PropertyGroup<TA, TB> CheckApplyAction(PropertyGroup<TA, TB> propertyGroup)
        {
            if (propertyGroup.CheckConflict(_preconditionPropertyGroup)) return null;
            OnPerformedActions();
            return propertyGroup + _effectPropertyGroup;
        }

        protected virtual void OnPerformedActions()
        {
            PerformedActions?.Invoke();
        }
        
        //Overrides
        public override string ToString()
        {
            return _id + " ->\n" + _effectPropertyGroup;
        }
    }
}