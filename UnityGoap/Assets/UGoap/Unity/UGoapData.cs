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
                    if(!goal.GetState().GetConflictConditions(key, worldState)) continue;
                    switch (GetPropertyType(key))
                    {
                        case Integer:
                            if (worldState.HasKey(key))
                                heuristic += Math.Abs((int)goal[key] - (int)worldState[key]);
                            else heuristic += (int)goal[key];
                            break;
                        case Float:
                            if (worldState.HasKey(key))
                                heuristic += (int)Mathf.Abs((float)goal[key] - (float)worldState[key]);
                            else heuristic += (int)goal[key];
                            break;
                        default:
                            if (!worldState.HasKey(key) || !goal[key].Equals(worldState[key])) 
                                heuristic += 1;
                            break;
                    }
                }
                return heuristic;
            };
        }
    }
}
