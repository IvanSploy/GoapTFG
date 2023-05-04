using System;
using System.Collections.Generic;
using UnityEngine;
using GoapTFG.Base;
using GoapTFG.Unity.ScriptableObjects;
using static GoapTFG.Unity.PropertyManager;
using static GoapTFG.Unity.PropertyManager.PropertyType;

namespace GoapTFG.Unity
{
    public static class GoapData
    {
        /// <summary>
        /// User defined heuristic for GOAP.
        /// </summary>
        /// <returns></returns>
        public static Func<GoapGoal<PropertyList, object>, PropertyGroup<PropertyList, object>, int> GetCustomHeuristic()
        {
            return null;
            return (goal, worldState) =>
            {
                var heuristic = 0;
                foreach (var key in goal.GetState().GetKeys())
                {
                    if(!worldState.HasConflict(key, goal.GetState())) continue;
                    switch (PropertyManager.GetType(key))
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
