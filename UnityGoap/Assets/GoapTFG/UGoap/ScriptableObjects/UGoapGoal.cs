using System;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;
using static GoapTFG.UGoap.CodeGenerator.EnumGenerator;

namespace GoapTFG.UGoap.ScriptableObjects
{
    [CreateAssetMenu(fileName = "Goal", menuName = "Goap Items/Goal", order = 2)]
    public class UGoapGoal : ScriptableObject
    {
        public static bool GenerateGoalNames;
        [HideInInspector] public List<ConditionProperty> goalProperties;
        
        public GoapGoal<PropertyList, object> Create(int priority)
        {
            PropertyGroup<PropertyList, object> state = new();
            AddIntoPropertyGroup(goalProperties, in state);
            return new GoapGoal<PropertyList, object>(name, state, priority);
        }
        
        private void Awake()
        {
            if (GenerateGoalNames) CreateGoalEnum();
        }
    }
    
    [Serializable]
    public struct GoapPriorityGoal
    {
        [SerializeField] private UGoapGoal goal;

        [Range(0, 15)] [SerializeField] private int priority;

        public GoapGoal<PropertyList, object> Create()
        {
            return goal.Create(priority);
        }
    }
}
