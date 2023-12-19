using System;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using UnityEngine.Serialization;
using static GoapTFG.UGoap.UGoapPropertyManager;
using static GoapTFG.UGoap.CodeGenerator.EnumGenerator;

namespace GoapTFG.UGoap.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Goal", menuName = "Goap Items/Goal", order = 2)]
    public class UGoapPriorityGoal : ScriptableObject
    {
        [FormerlySerializedAs("goalProperties")] [HideInInspector] public List<ConditionProperty> properties;
        
        public GoapGoal<PropertyKey, object> Create(int priority)
        {
            PropertyGroup<PropertyKey, object> state = new();
            AddIntoPropertyGroup(properties, in state);
            return new GoapGoal<PropertyKey, object>(name, state, priority);
        }
    }
    
    [Serializable]
    public struct PriorityGoal
    {
        [FormerlySerializedAs("goalSo")] [FormerlySerializedAs("goal")] [SerializeField] private UGoapPriorityGoal priorityGoal;

        [Range(0, 10)] [SerializeField] private int priority;

        public GoapGoal<PropertyKey, object> Create()
        {
            return priorityGoal.Create(priority);
        }
    }
}
