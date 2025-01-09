using System;
using System.Collections.Generic;
using LUGoap.Base;
using UnityEngine;
using UnityEngine.Serialization;

namespace LUGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Goal", menuName = "LUGoap/Goal")]
    public class GoalConfig : ScriptableObject
    {
        [FormerlySerializedAs("properties")] 
        [HideInInspector] public List<PropertyManager.ConditionProperty> Properties;
        
        public UGoal Create(int priority)
        {
            Conditions state = new();
            state.ApplyProperties(Properties);
            return new UGoal(name, state, priority);
        }
    }
    
    [Serializable]
    public struct PriorityGoal
    {
        [FormerlySerializedAs("priorityGoal")] 
        [SerializeField] private GoalConfig _goalConfig;
        [FormerlySerializedAs("priority")] 
        [Range(0, 10)] [SerializeField] private int _priority;

        public UGoal Create()
        {
            return _goalConfig.Create(_priority);
        }
    }
}
