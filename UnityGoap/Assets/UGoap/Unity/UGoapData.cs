using System;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.UGoapPropertyManager;
using static UGoap.Base.UGoapPropertyManager.PropertyType;

namespace UGoap.Unity
{
    public static class UGoapData
    {
        /// <summary>
        /// User defined heuristic for GOAP.
        /// </summary>
        /// <returns></returns>
        public static Func<GoapConditions, GoapState, int> GetCustomHeuristic()
        {
            //return null;
            return (goal, worldState) =>
            {
                var heuristic = 0;
                foreach (var goalPair in goal)
                {
                    PropertyKey key = goalPair.Key;
                    if (goal.GetConflictConditions(key, worldState).Count == 0) continue;
                    foreach (var conditionValue in goal[key])
                    {
                        if (worldState.HasKey(key))
                        {
                            if (conditionValue.Evaluate(worldState[key])) continue;
                        }

                        switch (GetPropertyType(key))
                        {
                            case Integer:
                                if (worldState.HasKey(key))
                                    heuristic += Math.Abs((int)conditionValue.Value - (int)worldState[key]);
                                else heuristic += (int)conditionValue.Value;
                                break;
                            case Float:
                                if (worldState.HasKey(key))
                                    heuristic +=
                                        (int)Mathf.Abs((float)conditionValue.Value - (float)worldState[key]);
                                else heuristic += (int)conditionValue.Value;
                                break;
                            default:
                                if (!worldState.HasKey(key) || !conditionValue.Equals(worldState[key]))
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
