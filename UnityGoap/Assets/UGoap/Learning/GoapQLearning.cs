using System.Collections.Generic;
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
        private const int Range = 500;
        
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

        public static int ParseToStateCode(GoapState goapState)
        {
            GoapState filteredGoapState = new GoapState(goapState);
            foreach (var pair in goapState)
            {
                var result = pair.Value switch
                {
                    int iValue => iValue / Range * Range,
                    float fValue => Mathf.Floor(fValue / Range) * Range,
                    _  => pair.Value,
                };
                filteredGoapState[pair.Key] = result;      
            }
            return filteredGoapState.GetHashCode();
        }

        public static int GetReward(Node startNode, Node finishNode)
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