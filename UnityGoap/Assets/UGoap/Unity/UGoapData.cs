using System;
using UnityEngine;
using GoapTFG.Base;
using static GoapTFG.UGoap.UGoapPropertyManager;
using static GoapTFG.UGoap.UGoapPropertyManager.PropertyType;

namespace GoapTFG.UGoap
{
    public static class UGoapData
    {
        /// <summary>
        /// User defined heuristic for GOAP.
        /// </summary>
        /// <returns></returns>
        public static Func<GoapGoal<PropertyKey, object>, PropertyGroup<PropertyKey, object>, int> GetCustomHeuristic()
        {
            //return null;
            return (goal, worldState) =>
            {
                var heuristic = 0;
                foreach (var key in goal.GetState().GetKeys())
                {
                    if(!worldState.HasConflict(key, goal.GetState())) continue;
                    switch (GetPropertyType(key))
                    {
                        case Integer:
                            if (worldState.HasKey(key))
                                heuristic += Math.Abs((int)goal.GetState().GetValue(key) - (int)worldState.GetValue(key));
                            else heuristic += (int)goal.GetState().GetValue(key);
                            break;
                        case Float:
                            if (worldState.HasKey(key))
                                heuristic += (int)Mathf.Abs((float)goal.GetState().GetValue(key) - (float)worldState.GetValue(key));
                            else heuristic += (int)goal.GetState().GetValue(key);
                            break;
                        default:
                            if (!worldState.HasKey(key) || !goal.GetState().GetValue(key).Equals(worldState.GetValue(key))) 
                                heuristic += 1;
                            break;
                    }
                }
                return heuristic;
            };
        }
    }
}