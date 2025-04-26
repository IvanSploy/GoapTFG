using System;
using System.Collections.Generic;
using QGoap.Base;
using UnityEngine;

namespace QGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Goal", menuName = "QGoap/Goal")]
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
