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
        
        public UGoapGoal Create(int priority)
        {
            GoapConditions state = new();
            AddIntoPropertyGroup(properties, in state);
            return new UGoapGoal(name, state, priority);
        }
    }
    
    [Serializable]
    public struct PriorityGoal
    {
        [SerializeField] private UGoapPriorityGoal priorityGoal;
        [Range(0, 10)] [SerializeField] private int priority;

        public UGoapGoal Create()
        {
            return priorityGoal.Create(priority);
        }
    }
}
