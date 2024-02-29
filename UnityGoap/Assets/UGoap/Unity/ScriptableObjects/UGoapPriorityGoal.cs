using System;
using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using static UGoap.Unity.UGoapPropertyManager;

namespace UGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Goal", menuName = "Goap Items/Goal", order = 2)]
    public class UGoapPriorityGoal : ScriptableObject
    {
        [HideInInspector] 
        public List<ConditionProperty> properties;
        
        public GoapGoal<PropertyKey, object> Create(int priority)
        {
            GoapConditions<PropertyKey, object> state = new();
            AddIntoPropertyGroup(properties, in state);
            return new GoapGoal<PropertyKey, object>(name, state, priority);
        }
    }
    
    [Serializable]
    public struct PriorityGoal
    {
        [SerializeField] private UGoapPriorityGoal priorityGoal;
        [Range(0, 10)] [SerializeField] private int priority;

        public GoapGoal<PropertyKey, object> Create()
        {
            return priorityGoal.Create(priority);
        }
    }
}
