using System;
using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Planner;
using GoapTFG.UGoap.ScriptableObjects;
using UnityEngine;
using static GoapTFG.UGoap.UGoapData;
using static GoapTFG.UGoap.UGoapPropertyManager;
using Random = UnityEngine.Random;

namespace GoapTFG.Base
{
    public struct GoapActionData<TKey, TValue>
    {
        public IGoapAction<TKey, TValue> Action;
        public GoapGoal<TKey, TValue> Goal;
        public PropertyGroup<TKey, TValue> ProceduralEffects;

        public GoapActionData(IGoapAction<TKey, TValue> action, GoapGoal<TKey, TValue> goal, PropertyGroup<TKey, TValue> proceduralEffects)
        {
            Action = action;
            Goal = goal;
            ProceduralEffects = proceduralEffects;
        }
    }
}