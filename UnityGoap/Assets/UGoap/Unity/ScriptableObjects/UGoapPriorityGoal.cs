using System;
using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Goal", menuName = "Goap Items/Goal", order = 2)]
    public class UGoapPriorityGoal : ScriptableObject
    {
        [HideInInspector] 
        public List<ConditionProperty> properties;
        
        public GoapGoal Create(int priority)
        {
            GoapConditions state = new();
            AddIntoPropertyGroup(properties, in state);
            return new GoapGoal(name, state, priority);
        }
    }
    
    [Serializable]
    public struct PriorityGoal
    {
        [SerializeField] private UGoapPriorityGoal priorityGoal;
        [Range(0, 10)] [SerializeField] private int priority;

        public GoapGoal Create()
        {
            return priorityGoal.Create(priority);
        }
    }
}
