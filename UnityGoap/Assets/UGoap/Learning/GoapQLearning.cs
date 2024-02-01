using System.Collections.Generic;
using System.Linq;
using UGoap.Base;
using UGoap.Planner;
using UGoap.Unity.ScriptableObjects;

namespace UGoap.Learning
{
    public static class GoapQLearning<TKey, TValue>
    {
        private static int _initialValue = 0;
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
                actionValues[action] = _initialValue;
            }
            else
            {
                _qValues[state] = new Dictionary<string, float> { { action, _initialValue } };
            }

            return _qValues[state][action];
        }
        
        private static float GetMaxQValue(int state)
        {
            return _qValues[state].Max(value => value.Value);
        }

        private static void SetQValue(int state, string action, float qValue)
        {
            if (_qValues.TryGetValue(state, out var actionValues))
            {
                actionValues[action] = _initialValue;
            }
            else
            {
                _qValues[state] = new Dictionary<string, float> { { action, qValue } };
            }
        }

        public static int ParseNodeToState(Node<TKey, TValue> node)
        {
            StateGroup<TKey, TValue> filteredState = new StateGroup<TKey, TValue>();
            foreach (var pair in node.Goal)
            {
                if (node.State.HasKey(pair.Key))
                    filteredState[pair.Key] = node.State[pair.Key];
            }

            return filteredState.GetHashCode();
        }
    }
}