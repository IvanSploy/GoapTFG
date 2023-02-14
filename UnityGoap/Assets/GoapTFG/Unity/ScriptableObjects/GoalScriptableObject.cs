using System;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Goal", menuName = "Goap Items/Goal", order = 2)]
    public class GoalScriptableObject : ScriptableObject
    {
        [HideInInspector] public List<ConditionProperty> goalProperties;
        
        public Goal<PropertyList, object> Create(int priority)
        {
            PropertyGroup<PropertyList, object> state = new();
            AddIntoPropertyGroup(goalProperties, ref state);
            return new Goal<PropertyList, object>(name, state, priority);
        }
    }
}
