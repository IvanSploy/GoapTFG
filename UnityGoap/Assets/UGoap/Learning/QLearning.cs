using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UGoap.Base;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;
using static UGoap.Base.PropertyManager;
using Random = System.Random;

namespace UGoap.Learning
{
    public class QLearning
    {
        private static readonly Random Random = new();
        
        private readonly string _path;
        private Dictionary<int, Dictionary<string, float>> _qValues;
        private readonly Func<Dictionary<string, float>, string> _bestActionPolitic;
        
        private readonly QLearningData _learningData;
        public float SucceedReward { get; private set; }
        public float FailReward { get; private set; }

        public QLearning(string fileName, QLearningData data, float succeedReward, float failReward,
            Func<Dictionary<string, float>, string> bestActionPolitic = null)
        {
            _path = Application.persistentDataPath + "/LearningData/" + fileName + ".json";
            _learningData = data;
            SucceedReward = succeedReward;
            FailReward = failReward;
            _bestActionPolitic = bestActionPolitic ?? Politics.GetMax;
        }

        public void Load()
        {
            CreateFile();
            var text = File.ReadAllText(_path);
            _qValues = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<string, float>>>(text) ?? new();
        }

        public void Save()
        {
            CreateFile();
            File.WriteAllText(_path, JsonConvert.SerializeObject(_qValues, Formatting.Indented));
        }

        public void Clear()
        {
            _qValues.Clear();
            Save();
        }

        private void CreateFile()
        {
            if (File.Exists(_path)) return;
            var directory = Path.GetDirectoryName(_path);
            Directory.CreateDirectory(directory);
            File.Create(_path).Close();
        }
        
        public float Get(int state, string action)
        {
            if (_qValues.TryGetValue(state, out var actionValues))
            {
                if (actionValues.TryGetValue(action, out var qValue))
                {
                    return qValue;
                }
                actionValues[action] = 0;
            }
            else
            {
                _qValues[state] = new Dictionary<string, float> { { action, 0 } };
            }
            
            return _qValues[state][action];
        }

        public string GetBestAction(State state, Conditions conditions)
        {
            var learningCode = GetLearningCode(state, conditions);
            if (!_qValues.TryGetValue(learningCode, out var values)) return null;
            return _bestActionPolitic(values);
        }
        
        public float GetMaxValue(State state, Conditions conditions)
        {
            var learningCode = GetLearningCode(state, conditions);
            return GetMaxValue(learningCode);
        }
        
        public void Update(Conditions goal, State initialState, string actionName, float reward, State nextState)
        {
            var initialCode = GetLearningCode(initialState, goal);
            var finalCode = GetLearningCode(nextState, goal);
            Apply(initialCode, actionName, reward, finalCode);
        }
        
        //Exploration
        public bool IsExploring()
        {
            float randomExplore;
            lock (Random)
            {
                randomExplore = (float)Random.NextDouble();
            }
            return randomExplore < _learningData.ExploreChance;
        }

        public float GetExploreValue()
        {
            lock (Random)
            {
                var randomExplore = Random.NextDouble();
                return (float)(randomExplore * (_learningData.ExploreRange.y - _learningData.ExploreRange.x)
                       + _learningData.ExploreRange.x);
            }
        }
        
        private float Apply(int state, string action, float r, int newState)
        {
            var qValue = (1 - _learningData.Alpha) * Get(state, action) + _learningData.Alpha * (r + _learningData.Gamma * GetMaxValue(newState));
            Set(state, action, qValue);
            return qValue;
        }

        private void Set(int state, string action, float qValue)
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

        private float GetMaxValue(int state)
        {
            if (!_qValues.TryGetValue(state, out var values)) return 0;
            return values.Max(pair => pair.Value);
        }
        
        private int GetLearningCode(State state, Conditions conditions)
        {
            var distances = conditions.GetDistances(state, _learningData.LearningKeys);

            if (_learningData.ValueRange > 1)
            {
                foreach (var pair in distances)
                {
                    distances[pair.Key] = pair.Value / _learningData.ValueRange + 1;
                }
            }
            
            int hash = 17;
            foreach(KeyValuePair<PropertyKey, int> kvp in distances)
            {
                hash = hash * 31 + (kvp.Key.GetHashCode() ^ kvp.Value.GetHashCode());
            }
            return hash;
        }

        public override string ToString()
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

            return log;
        }
    }
}