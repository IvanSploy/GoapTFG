using System.Collections.Generic;
using System.Linq;
using UGoap.Base;
using UGoap.Planner;
using UnityEngine;

namespace UGoap.Learning
{
    [CreateAssetMenu(fileName = "QLearning", menuName = "Goap Items/QLearning", order = 1)]
    public class GoapQLearning : ScriptableObject, ISerializationCallbackReceiver, IQLearning
    {
        public int InitialValue = 0;
        [Range(0f,1f)]
        public float Alpha = 0.25f;
        [Range(0f,1f)]
        public float Gamma = 0.9f;
        public int ValueRange = 500;

        public TextAsset QValuesJson;
        public Dictionary<int, Dictionary<string, float>> QValues = new();
        
        public void OnAfterDeserialize()
        {
            if (QValuesJson != null)
            {
                // Deserialize JSON into QValues dictionary
                //JsonUtility.FromJsonOverwrite(QValuesJson.text, this);
            }
        }

        public void OnBeforeSerialize()
        {
            // Serialize QValues dictionary to JSON
            //QValuesJson = new TextAsset(JsonUtility.ToJson(this));
        }
        
        public float UpdateQValue(int state, string action, float r, int newState)
        {
            var qValue = (1 - Alpha) * GetQValue(state, action) + Alpha * r + Gamma * GetMaxQValue(newState);
            SetQValue(state, action, qValue);
            return qValue;
        }
        
        public float GetQValue(int state, string action)
        {
            if (QValues.TryGetValue(state, out var actionValues))
            {
                if (actionValues.TryGetValue(action, out var qValue))
                {
                    return qValue;
                }
                actionValues[action] = InitialValue;
            }
            else
            {
                QValues[state] = new Dictionary<string, float> { { action, InitialValue } };
            }

            return QValues[state][action];
        }
        
        private float GetMaxQValue(int state)
        {
            if (!QValues.ContainsKey(state)) return 0;
            return QValues[state].Max(value => value.Value);
        }

        private void SetQValue(int state, string action, float qValue)
        {
            if (QValues.TryGetValue(state, out var actionValues))
            {
                actionValues[action] = qValue;
            }
            else
            {
                QValues[state] = new Dictionary<string, float> { { action, qValue } };
            }
        }

        public int ParseToStateCode(GoapState goapState)
        {
            GoapState filteredGoapState = new GoapState(goapState);
            foreach (var pair in goapState)
            {
                var result = pair.Value switch
                {
                    int iValue => iValue / ValueRange * ValueRange,
                    float fValue => Mathf.Floor(fValue / ValueRange) * ValueRange,
                    _  => pair.Value,
                };
                filteredGoapState[pair.Key] = result;      
            }
            return filteredGoapState.GetHashCode();
        }

        public int GetReward(Node startNode, Node finishNode)
        {
            return startNode.TotalCost - finishNode.TotalCost;
        }

        public void DebugLearning()
        {
            string log = "";
            int generatedStates = 0;
            string morelog = "";
            foreach (var state in QValues)
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