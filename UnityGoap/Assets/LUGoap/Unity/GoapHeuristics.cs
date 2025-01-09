using System;
using LUGoap.Base;
using UnityEngine;
using static LUGoap.Base.PropertyManager;
using static LUGoap.Base.PropertyManager.PropertyType;

namespace LUGoap.Unity
{
    public static class GoapHeuristics
    {
        /// <summary>
        /// User defined heuristic for GOAP.
        /// </summary>
        /// <returns></returns>
        public static Func<Conditions, State, int> GetCustomHeuristic()
        {
            //return null;
            return (goal, worldState) =>
            {
                var heuristic = 0;
                foreach (var goalPair in goal)
                {
                    PropertyKey key = goalPair.Key;
                    var condition = goal.GetConflictCondition(key, worldState);
                    if (condition == null) continue;
                    
                    foreach (var conditionValue in goal[key])
                    {
                        if (worldState.Has(key))
                        {
                            if (conditionValue.Evaluate(worldState[key])) continue;
                        }

                        switch (GetPropertyType(key))
                        {
                            case Integer:
                                if (worldState.Has(key))
                                    heuristic += Math.Abs((int)conditionValue.Value - (int)worldState[key]);
                                else heuristic += (int)conditionValue.Value;
                                break;
                            case Float:
                                if (worldState.Has(key))
                                    heuristic +=
                                        (int)Mathf.Abs((float)conditionValue.Value - (float)worldState[key]);
                                else heuristic += (int)conditionValue.Value;
                                break;
                            default:
                                if (!worldState.Has(key) || !conditionValue.Equals(worldState[key]))
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
