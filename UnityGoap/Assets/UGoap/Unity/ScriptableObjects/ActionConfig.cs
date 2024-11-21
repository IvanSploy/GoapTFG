using System;
using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.ScriptableObjects
{
    public abstract class ActionConfig<TAction> : ActionConfig where TAction : GoapAction, new()
    {
        protected override GoapAction CreateActionBase() => Install(new TAction());
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

        public GoapAction Create()
        {
            var goapAction = CreateActionBase();
            
            var preconditions = new GoapConditions();
            preconditions.ApplyProperties(Preconditions);
            
            var effects = new GoapEffects();
            effects.ApplyProperties(Effects);
            
            goapAction.Initialize(name, preconditions, effects);

            return goapAction;
        }

        protected abstract GoapAction CreateActionBase();
    }
}
