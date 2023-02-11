using System;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity
{
    [CreateAssetMenu(fileName = "Goal", menuName = "Goap Items/Goal", order = 2)]
    public class GoalScriptableObject : ScriptableObject
    {
        [HideInInspector] public string nameItem;
        [HideInInspector] public List<ConditionProperty> goalProperties;
    
        private void Awake()
        {
            nameItem = name;
        }
        
        private void OnValidate()
        {
            if(nameItem.Equals(""))nameItem = name;
        }
        
        public Goal<string, object> Create(int priority)
        {
            PropertyGroup<string, object> state = new();
            AddIntoPropertyGroup(goalProperties, ref state);
            return new Goal<string, object>(nameItem, state, priority);
        }
    }
}
