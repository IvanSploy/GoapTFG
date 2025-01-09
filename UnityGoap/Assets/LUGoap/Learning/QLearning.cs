using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LUGoap.Base;
using Unity.Plastic.Newtonsoft.Json;
using static LUGoap.Base.PropertyManager;
using Action = LUGoap.Base.Action;
using Random = LUGoap.Base.Random;

namespace LUGoap.Learning
{
    public class QLearning
    {
        private readonly string _path;
        private readonly string _file;
        private Dictionary<int, Dictionary<string, float>> _qValues;
        private readonly Func<Dictionary<string, float>, string> _bestActionPolitic;
        
        private readonly QLearningData _learningData;
        public float SucceedReward { get; private set; }
        public float FailReward { get; private set; }

        public QLearning(string dataPath, string fileName, QLearningData data, float succeedReward, float failReward,
            Func<Dictionary<string, float>, string> bestActionPolitic = null)
        {
            _path = dataPath;
            _file = fileName + ".json";
            _learningData = data;
            SucceedReward = succeedReward;
            FailReward = failReward;
            _bestActionPolitic = bestActionPolitic ?? Politics.GetMax;
        }

        public void Load()
        {
            CreateFile();
            var fullPath = Path.Combine(_path, _file);
            var text = File.ReadAllText(fullPath);
            _qValues = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<string, float>>>(text) ?? new();
        }

        public void Save()
        {
            CreateFile();
            var fullPath = Path.Combine(_path, _file);
            File.WriteAllText(fullPath, JsonConvert.SerializeObject(_qValues, Formatting.Indented));
        }

        public void Clear()
        {
            _qValues.Clear();
            Save();
        }

        private void CreateFile()
        {
            var fullPath = Path.Combine(_path, _file);
            if (File.Exists(fullPath)) return;
            Directory.CreateDirectory(_path);
            File.Create(fullPath).Close();
        }
        
        public int GetLearningCode(State state, Conditions conditions)
        {
            var distances = conditions.GetDistances(state, _learningData.FilterKeys, _learningData.AdditionalKeys);
            if(distances.Count == 0) return 0;
            
            if (_learningData.ValueRange > 1)
            {
                foreach (var pair in distances.ToList())
                {
                    distances[pair.Key] = pair.Value / _learningData.ValueRange + 1;
                }
            }

            var list = distances.ToList();
            list.Sort((comp1, comp2) => comp1.Key.CompareTo(comp2.Key));

            int hash = 17;
            foreach(KeyValuePair<PropertyKey, int> kvp in list)
            {
                hash = hash * 31 + (kvp.Key.GetHashCode() ^ kvp.Value.GetHashCode());
            }
            return hash;
        }

        public string GetBestAction(int learningCode)
        {
            if (!_qValues.TryGetValue(learningCode, out var values)) return null;
            return _bestActionPolitic(values);
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
        
        public float GetMax(int state)
        {
            if (!_qValues.TryGetValue(state, out var values)) return 0;
            return values.Max(pair => pair.Value);
        }
        
        public void Update(int learningCode, string action, float r, int nextLearningCode)
        {
            var qValue = (1 - _learningData.Alpha) * Get(learningCode, action) + _learningData.Alpha * (r + _learningData.Gamma * GetMax(nextLearningCode));
            Set(learningCode, action, qValue);
        }
        
        //Exploration
        public bool IsExploring()
        {
            float randomExplore = Random.Next();
            return randomExplore < _learningData.ExploreChance;
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