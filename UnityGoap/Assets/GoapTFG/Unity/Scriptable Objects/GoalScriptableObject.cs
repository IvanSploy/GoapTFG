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
        [HideInInspector] public string goalName;
        [HideInInspector] public List<ConditionProperty> goalProperties;
    
        private void Awake()
        {
            goalName = name;
        }
        
        private void OnValidate()
        {
            if(goalName is "")goalName = name;
        }
        
        public Goal<PropertyList, object> Create(int priority)
        {
            PropertyGroup<PropertyList, object> state = new();
            AddIntoPropertyGroup(goalProperties, ref state);
            return new Goal<PropertyList, object>(goalName, state, priority);
        }
    }
}
