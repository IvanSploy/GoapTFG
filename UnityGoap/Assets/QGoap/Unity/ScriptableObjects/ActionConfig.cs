using System;
using System.Collections.Generic;
using QGoap.Base;
using QGoap.Unity.Actions;
using UnityEngine;
using UnityEngine.Serialization;
using static QGoap.Base.PropertyManager;

namespace QGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Action", menuName = "LUGoap/Action")]
    public class ActionConfig : ActionBaseConfig
    {
        [SerializeReference] private Base.Action _actionData;

        protected override Base.Action CreateAction()
        {
            return _actionData ?? new DefaultAction();
        }

        [ContextMenu("FixReference")]
        public void FixReference()
        {
            _actionData = null;
        }
    }
    
    public abstract class ActionBaseConfig : ScriptableObject
    {
        [SerializeField] private int _cost = 1;
        [FormerlySerializedAs("Preconditions")] 
        [SerializeField] private List<ConditionProperty> _preconditions = new();
        [FormerlySerializedAs("Effects")] 
        [SerializeField] private List<EffectProperty> _effects = new();

        private void OnValidate()
        {
            _cost = Math.Max(0, _cost);
        }

        public Base.Action Create(IAgent agent)
        {
            var preconditions = new ConditionGroup();
            preconditions.ApplyProperties(_preconditions);

            var effects = new EffectGroup();
            effects.ApplyProperties(_effects);

            var action = CreateAction();
            action.Initialize(name, preconditions, effects, agent);
            action.SetCost(_cost);
            return action;
        }

        protected abstract Base.Action CreateAction();
    }
}
