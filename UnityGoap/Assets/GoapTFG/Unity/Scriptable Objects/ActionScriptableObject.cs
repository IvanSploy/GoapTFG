using System;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity
{
    [CreateAssetMenu(fileName = "Action", menuName = "Goap Items/Action", order = 3)]
    public class ActionScriptableObject : ScriptableObject
    {
        [HideInInspector] public string nameItem;
        [HideInInspector] public List<ConditionProperty> preconditions;
        [HideInInspector] public List<EffectProperty> effects;
        [HideInInspector] public int cost;
    
        private void Awake()
        {
            nameItem = name;
            cost = 1;
        }

        private void OnValidate()
        {
            cost = Math.Max(1, cost);
            if(nameItem.Equals(""))nameItem = name;
        }
    
        public Base.Action<string, object> Create()
        {
            PropertyGroup<string, object> precsPg = new();
            AddIntoPropertyGroup(preconditions, ref precsPg);
            PropertyGroup<string, object> effectsPg = new();
            AddIntoPropertyGroup(effects, ref effectsPg);
            Base.Action<string, object> action = new(nameItem, precsPg, effectsPg)
            {
                Cost = cost
            };
            ActionAdditionalData data = GoapData.GetActionAdditionalData(nameItem);
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
