﻿using System.Collections.Generic;
using System.Linq;
using UGoap.Base;
using UGoap.Planner;
using UnityEngine;

namespace UGoap.Learning
{
    public static class GoapQLearning
    {
        public static readonly int InitialValue = 0;
        private const float Alpha = 0.25f;
        private const float Gamma = 0.9f;
        
        private static Dictionary<int, Dictionary<string, float>> _qValues = new();
        
        public static float UpdateQValue(int state, string action, float r, int newState)
        {
            var qValue = (1 - Alpha) * GetQValue(state, action) + Alpha * r + Gamma * GetMaxQValue(newState);
            SetQValue(state, action, qValue);
            return qValue;
        }
        
        public static float GetQValue(int state, string action)
        {
            if (_qValues.TryGetValue(state, out var actionValues))
            {
                if (actionValues.TryGetValue(action, out var qValue))
                {
                    return qValue;
                }
                actionValues[action] = InitialValue;
            }
            else
            {
                _qValues[state] = new Dictionary<string, float> { { action, InitialValue } };
            }

            return _qValues[state][action];
        }
        
        private static float GetMaxQValue(int state)
        {
            if (!_qValues.ContainsKey(state)) return 0;
            return _qValues[state].Max(value => value.Value);
        }

        private static void SetQValue(int state, string action, float qValue)
        {
            if (_qValues.TryGetValue(state, out var actionValues))
            {
                actionValues[action] = qValue;
            }
            else
            {
                _qValues[state] = new Dictionary<string, float> { { action, qValue } };
            }
        }

        public static int ParseToStateCode<TKey, TValue>(StateGroup<TKey, TValue> state, GoapGoal<TKey,TValue> goal)
        {
            StateGroup<TKey, TValue> filteredState = new StateGroup<TKey, TValue>();
            foreach (var pair in goal)
            {
                if (state.HasKey(pair.Key))
                    filteredState[pair.Key] = state[pair.Key];
            }

            return filteredState.GetHashCode();
        }

        public static int GetReward<TKey, TValue>(Node<TKey, TValue> startNode, Node<TKey, TValue> finishNode)
        {
            return startNode.TotalCost - finishNode.TotalCost;
        }

        public static void DebugLearning()
        {
            string log = "";
            int generatedStates = 0;
            string morelog = "";
            foreach (var state in _qValues)
            {
                generatedStates++;
                morelog += state.Key + " | ";
                foreach (var actionValue in state.Value)
                {
                    morelog += actionValue.Key + ": " + actionValue.Value + "\n";
                }
            }
            log += "GeneratedStates states: " + generatedStates + "\n";
            log += morelog;

            Debug.Log(log);
        }
    }
}