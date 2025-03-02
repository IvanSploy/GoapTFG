using System;
using System.Collections.Generic;
using LUGoap.Base;
using UnityEngine;

namespace LUGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Goal", menuName = "LUGoap/Goal")]
    public class GoalConfig : ScriptableObject
    {
        [HideInInspector] public List<PropertyManager.ConditionProperty> Properties;
        
        public Goal Create()
        {
            ConditionGroup group = new();
            group.ApplyProperties(Properties);
            return new Goal(name, group);
        }
    }
}
