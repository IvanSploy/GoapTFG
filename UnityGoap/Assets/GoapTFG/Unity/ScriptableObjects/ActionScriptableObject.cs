using System;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Planner;
using UnityEngine;
using GoapTFG.Unity.CodeGenerator;
using static GoapTFG.Unity.GoapData;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Action", menuName = "Goap Items/Action", order = 3)]
    public class ActionScriptableObject : ScriptableObject
    {
        public static bool GenerateActionNames;
        [HideInInspector] public List<ConditionProperty> preconditions;
        [HideInInspector] public List<EffectProperty> effects;
        [HideInInspector] public int cost;

        private void Awake()
        {
            if (GenerateActionNames) EnumGenerator.CreateActionEnum();
        }

        private void OnValidate()
        {
            cost = Math.Max(0, cost);
        }
    
        public Base.Action<PropertyList, object> Create(IAgent<PropertyList, object> agent)
        {
            PropertyGroup<PropertyList, object> precsPg = new();
            AddIntoPropertyGroup(preconditions, ref precsPg);
            PropertyGroup<PropertyList, object> effectsPg = new();
            AddIntoPropertyGroup(effects, ref effectsPg);
            Base.Action<PropertyList, object> action = new(agent, name, precsPg, effectsPg)
            {
                Cost = cost
            };
            ActionAdditionalData data = GetActionAdditionalData(name);
            if (data != null)
            {
                action.SetCustomCost(data.customCost);
                action.ProceduralConditions += data.conditions;
                action.ProceduralEffects += data.effects;
                action.PerformedActions += data.actions;
            }
            return action;
        }
    }
}
