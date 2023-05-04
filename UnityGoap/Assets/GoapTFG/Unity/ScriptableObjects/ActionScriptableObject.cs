using System;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Planner;
using UnityEngine;
using static GoapTFG.Unity.CodeGenerator.EnumGenerator;
using static GoapTFG.Unity.GoapData;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity.ScriptableObjects
{
    public class ActionScriptableObject : ScriptableObject
    {
        public static bool GenerateActionNames;
        [HideInInspector] public List<ConditionProperty> preconditions;
        [HideInInspector] public List<EffectProperty> effects;
        [HideInInspector] public int cost;

        private void Awake()
        {
            if (GenerateActionNames) CreateActionEnum();
        }

        private void OnValidate()
        {
            cost = Math.Max(0, cost);
        }
    
        /*public Base.GoapAction<PropertyList, object> Create(IAgent<PropertyList, object> agent)
        {
            PropertyGroup<PropertyList, object> precsPg = new();
            AddIntoPropertyGroup(preconditions, ref precsPg);
            PropertyGroup<PropertyList, object> effectsPg = new();
            AddIntoPropertyGroup(effects, ref effectsPg);
            Base.GoapAction<PropertyList, object> goapAction = new(agent, name, precsPg, effectsPg)
            {
                Cost = cost
            };
            ActionAdditionalData data = GetActionAdditionalData(name);
            if (data != null)
            {
                goapAction.SetCustomCost(data.CustomCost);
                goapAction.ProceduralConditions += data.Conditions;
                goapAction.ProceduralEffects += data.Effects;
                goapAction.PerformedActions += data.Actions;
            }
            return goapAction;
        }*/
    }
}
