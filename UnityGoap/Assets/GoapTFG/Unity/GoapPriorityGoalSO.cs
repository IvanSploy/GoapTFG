using System;
using GoapTFG.Base;
using GoapTFG.Unity.ScriptableObjects;
using UnityEngine;

namespace GoapTFG.Unity
{
    [Serializable]
    public struct GoapPriorityGoalSO
    {
        [SerializeField] private GoalScriptableObject goal;

        [Range(0, 15)] [SerializeField] private int priority;

        public GoapGoal<PropertyManager.PropertyList, object> Create()
        {
            return goal.Create(priority);
        }
    }
}