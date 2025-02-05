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
        
        public Goal Create(int priority)
        {
            Conditions state = new();
            state.ApplyProperties(Properties);
            return new Goal(name, state, priority);
        }
    }
    
    [Serializable]
    public struct PriorityGoal
    {
        [SerializeField] private GoalConfig _goalConfig;
        [Range(0, 10)] [SerializeField] private int _priority;

        public Goal Create()
        {
            return _goalConfig.Create(_priority);
        }
    }
}
