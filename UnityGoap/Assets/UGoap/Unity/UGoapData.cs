using System;
using UGoap.Base;
using UnityEngine;
using static UGoap.Unity.UGoapPropertyManager;
using static UGoap.Unity.UGoapPropertyManager.PropertyType;

namespace UGoap.Unity
{
    public static class UGoapData
    {
        /// <summary>
        /// User defined heuristic for GOAP.
        /// </summary>
        /// <returns></returns>
        public static Func<GoapGoal<PropertyKey, object>, StateGroup<PropertyKey, object>, int> GetCustomHeuristic()
        {
            //return null;
            return (goal, worldState) =>
            {
                var heuristic = 0;
                foreach (var goalPair in goal)
                {
                    PropertyKey key = goalPair.Key;
                    if(!goal.GetState().HasConflict(key, worldState)) continue;
                    foreach (var conditionValue in goal[key])
                    {
                        if (worldState.HasKey(key))
                        {
                            if (BaseTypes.EvaluateCondition(worldState[key].Value, conditionValue.Value,
                                    conditionValue.ConditionType)) continue;
                        }

                        switch (GetPropertyType(key))
                        {
                            case Integer:
                                if (worldState.HasKey(key))
                                    heuristic += Math.Abs((int)conditionValue.Value - (int)worldState[key].Value);
                                else heuristic += (int)conditionValue.Value;
                                break;
                            case Float:
                                if (worldState.HasKey(key))
                                    heuristic += (int)Mathf.Abs((float)conditionValue.Value - (float)worldState[key].Value);
                                else heuristic += (int)conditionValue.Value;
                                break;
                            default:
                                if (!worldState.HasKey(key) || !conditionValue.Equals(worldState[key].Value)) 
                                    heuristic += 1;
                                break;
                        }
                    }
                }
                return heuristic;
            };
        }
    }
}
