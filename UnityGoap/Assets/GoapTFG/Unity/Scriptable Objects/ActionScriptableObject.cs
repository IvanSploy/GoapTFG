using System;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Planner;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity
{
    [CreateAssetMenu(fileName = "Action", menuName = "Goap Items/Action", order = 3)]
    public class ActionScriptableObject : ScriptableObject
    {
        [HideInInspector] public string actionName;
        [HideInInspector] public List<ConditionProperty> preconditions;
        [HideInInspector] public List<EffectProperty> effects;
        [HideInInspector] public int cost;
    
        private void Awake()
        {
            actionName = name;
            cost = 1;
        }

        private void OnValidate()
        {
            cost = Math.Max(1, cost);
            if(actionName.Equals(""))actionName = name;
        }
    
        public Base.Action<PropertyList, object> Create(IAgent<PropertyList, object> agent)
        {
            PropertyGroup<PropertyList, object> precsPg = new();
            AddIntoPropertyGroup(preconditions, ref precsPg);
            PropertyGroup<PropertyList, object> effectsPg = new();
            AddIntoPropertyGroup(effects, ref effectsPg);
            Base.Action<PropertyList, object> action = new(agent, actionName, precsPg, effectsPg)
            {
                Cost = cost
            };
            ActionAdditionalData data = GoapData.GetActionAdditionalData(actionName);
            if (data != null)
            {
                action.ProceduralConditions += data.conditions;
                action.ProceduralEffects += data.effects;
                action.PerformedActions += data.actions;
            }
            return action;
        }
    }
}
