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
        [HideInInspector] public List<UGoapPropertyManager.ConditionProperty> goalProperties;
        
        public GoapGoal<UGoapPropertyManager.PropertyList, object> Create(int priority)
        {
            PropertyGroup<UGoapPropertyManager.PropertyList, object> state = new();
            AddIntoPropertyGroup(goalProperties, in state);
            return new GoapGoal<UGoapPropertyManager.PropertyList, object>(name, state, priority);
        }
        
        private void Awake()
        {
            if (GenerateGoalNames) CreateGoalEnum();
        }
    }
    
    [Serializable]
    public struct GoapPriorityGoal
    {
        [SerializeField] private UGoapGoal uGoapGoal;

        [Range(0, 15)] [SerializeField] private int priority;

        public GoapGoal<UGoapPropertyManager.PropertyList, object> Create()
        {
            return uGoapGoal.Create(priority);
        }
    }
}
