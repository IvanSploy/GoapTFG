using System.Collections.Generic;
using System.IO;
using System.Linq;
using UGoap.Base;
using Unity.Plastic.Newtonsoft.Json;
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
        
        [Header("Reward")] 
        [Range(1,500)] public float PositiveMultiplier;
        [Range(1,500)] public float NegativeMultiplier;
        
        [Header("Save")]
        public string FileName;
        
        private Dictionary<int, Dictionary<string, float>> _qValues = new();

        private string Path => Application.dataPath + "\\" + FileName + ".json";

        public void OnAfterDeserialize()
        {
            // Deserialize JSON into _qValues dictionary
            var text = File.ReadAllText(Path);
            _qValues = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<string, float>>>(text);
        }

        public void OnBeforeSerialize()
        {
            // Serialize _qValues dictionary to JSON
            if (!File.Exists(Path))
            {
                File.Create(Path).Close();
            }

            File.WriteAllText(Path, JsonConvert.SerializeObject(_qValues));
        }

        [ContextMenu("Clear Data")]
        public void Clear()
        {
            _qValues.Clear();
            OnBeforeSerialize();
        }
        
        public float UpdateQValue(int state, string action, float r, int newState)
        {
            var qValue = (1 - Alpha) * GetQValue(state, action) + Alpha * (r + Gamma * GetMaxQValue(newState));
            SetQValue(state, action, qValue);
            return qValue;
        }
        
        public float GetQValue(int state, string action)
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
        
        private float GetMaxQValue(int state)
        {
            if (!_qValues.ContainsKey(state)) return 0;
            return _qValues[state].Max(value => value.Value);
        }

        private void SetQValue(int state, string action, float qValue)
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

        public void DebugLearning()
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