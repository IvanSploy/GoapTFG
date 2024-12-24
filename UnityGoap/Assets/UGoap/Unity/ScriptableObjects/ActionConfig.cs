using System;
using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.PropertyManager;

namespace UGoap.Unity.ScriptableObjects
{
    public abstract class ActionConfig<TAction> : ActionConfig where TAction : Base.Action, new()
    {
        protected override Base.Action CreateActionBase() => Install(new TAction());
        protected abstract TAction Install(TAction action);
    }
    
    public abstract class ActionConfig : ScriptableObject
    {
        [Header("Main")]
        [SerializeField] private int _cost = 1;
        [HideInInspector] public List<ConditionProperty> Preconditions = new();
        [HideInInspector] public List<EffectProperty> Effects = new();
        
        //Updating data from the scriptable object.
        private void OnValidate()
        {
            _cost = Math.Max(0, _cost);
        }

        public Base.Action Create()
        {
            var goapAction = CreateActionBase();
            
            var preconditions = new Conditions();
            preconditions.ApplyProperties(Preconditions);
            
            var effects = new Effects();
            effects.ApplyProperties(Effects);
            
            goapAction.Initialize(name, preconditions, effects);

            return goapAction;
        }

        protected abstract Base.Action CreateActionBase();
    }
}
