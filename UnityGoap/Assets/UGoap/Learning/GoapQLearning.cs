using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UGoap.Base;
using UGoap.Planner;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

namespace UGoap.Learning
{
    [CreateAssetMenu(fileName = "QLearning", menuName = "Goap Items/QLearning", order = 1)]
    public class GoapQLearning : ScriptableObject, IQLearning
    {
        [Header("Config")] 
        public Vector2 InitialRange = new(-500,500);
        [Range(0f,1f)]
        public float Alpha = 0.25f;
        [Range(0f,1f)]
        public float Gamma = 0.9f;
        public int ValueRange = 500;
        public List<UGoapPropertyManager.PropertyKey> LearningKeys;

        [SerializeField] private LearningType _type;
        public LearningType Type => _type;

        [Header("Reward")] 
        public float PositiveReward;
        public float PositiveRewardDecay;
        public float NegativeReward;
        public float NegativeRewardDecay;
        
        [Header("Explore")] 
        [Range(0f,1f)]
        public float ExploreChance;
        
        public float MinExploreValue = 0;
        public float MaxExploreValue = 1000;
        
        [Header("Save")]
        public string FileName;
        
        private Dictionary<int, Dictionary<string, float>> _qValues = new();
        internal Dictionary<int, Dictionary<string, float>> Values => _qValues;
        private string Path => Application.dataPath + "\\" + FileName + ".json";
        private float _explorationValue;

        public void OnEnable()
        {
            if (!File.Exists(Path))
            {
                File.Create(Path).Close();
            }
            
            // Deserialize JSON into _qValues dictionary
            var text = File.ReadAllText(Path);
            _qValues = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<string, float>>>(text) ?? new();
        }

        public void OnDisable()
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
            OnDisable();
        }

        public float Apply(int state, string action, float r, int newState)
        {
            var qValue = (1 - Alpha) * GetQValue(state, action) + Alpha * (r + Gamma * GetMaxQValue(newState));
            SetQValue(state, action, qValue);
            return qValue;
        }
        
        public float Get(int state, string action)
        {
            //Exploration
            var randomExplore = Random.Range(0f, 1f);
            if (randomExplore < ExploreChance)
            {
                return Random.Range(MinExploreValue, MaxExploreValue);
            }

            return GetQValue(state, action);
        }
        
        public void UpdateLearning(Node node, GoapState initialState, float reward)
        {
            int initialNode;
            int finishNode;
            switch (Type)
            {
                case LearningType.State:
                    initialNode = ParseToStateCode(initialState);
                    finishNode = initialNode;
                    break;
                case LearningType.Goal:
                    initialNode = ParseToStateCode(node.Parent.Goal);
                    finishNode = ParseToStateCode(node.Goal);
                    break;
                case LearningType.Both:
                    initialNode = ParseToStateCode(initialState, node.Parent.Goal);
                    finishNode = ParseToStateCode(initialState, node.Goal);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            Apply(initialNode, node.PreviousActionInfo.Name, reward, finishNode);
        }

        private float GetRandom() => Random.Range(InitialRange.x, InitialRange.y);
        
        private float GetQValue(int state, string action)
        {
            if (_qValues.TryGetValue(state, out var actionValues))
            {
                if (actionValues.TryGetValue(action, out var qValue))
                {
                    return qValue;
                }
                actionValues[action] = GetRandom();
            }
            else
            {
                _qValues[state] = new Dictionary<string, float> { { action, GetRandom() } };
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

        public int ParseToStateCode(GoapConditions goal)
        {
            GoapConditions filteredGoal = new GoapConditions();
            foreach (var pair in goal)
            {
                if(!LearningKeys.Contains(pair.Key)) continue;
                foreach (var condition in pair.Value)
                {
                    var result = condition.Value switch
                    {
                        int iValue => iValue / ValueRange * ValueRange,
                        float fValue => Mathf.Floor(fValue / ValueRange) * ValueRange,
                        _  => condition.Value,
                    };
                    filteredGoal.Set(pair.Key, condition.ConditionType, result);  
                }
            }
            return filteredGoal.GetHashCode();
        }
        
        public int ParseToStateCode(GoapState state)
        {
            GoapState filteredState = new GoapState();
            foreach (var pair in state)
            {
                if(!LearningKeys.Contains(pair.Key)) continue;
                var result = pair.Value switch
                {
                    int iValue => iValue / ValueRange * ValueRange,
                    float fValue => Mathf.Floor(fValue / ValueRange) * ValueRange,
                    _  => pair.Value,
                };
                filteredState.Set(pair.Key, result);
            }
            return filteredState.GetHashCode();
        }
        
        public int ParseToStateCode(GoapState state, GoapConditions goal)
        {
            int hashState = ParseToStateCode(state);
            int hashGoal = ParseToStateCode(goal);
            return hashState + hashGoal;
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

                morelog += "\n";
            }
            log += "GeneratedStates states: " + generatedStates + "\n";
            log += morelog;

            Debug.Log(log);
        }
    }
}