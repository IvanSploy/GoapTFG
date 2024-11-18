using System;
using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using UnityEngine.Serialization;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.ScriptableObjects
{
    public class ActionConfig : ScriptableObject
    {
        [Header("Main")]
        [SerializeField] private int _cost = 1;
        [FormerlySerializedAs("Preconditions")] 
        [HideInInspector] public List<ConditionProperty> _preconditions = new();
        [FormerlySerializedAs("Effects")] 
        [HideInInspector] public List<EffectProperty> _effects = new();

        [SubclassSelector(typeof(GoapAction)), SerializeField] private string _actionType;
        public GoapAction GoapAction;
        
        //Updating data from the scriptable object.
        private void OnValidate()
        {
            _cost = Math.Max(0, _cost);
        }

        public void Create()
        {
            Type actionType = Type.GetType(_actionType);
            if (actionType == null) return;
            
            var preconditions = new GoapConditions();
            preconditions.ApplyProperties(_preconditions);
            
            var effects = new GoapEffects();
            effects.ApplyProperties(_effects);
            
            GoapAction = (GoapAction) Activator.CreateInstance(actionType, name, preconditions, effects);
        }
    }
}
