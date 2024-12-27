using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UGoap.Base;
using Unity.Plastic.Newtonsoft.Json;
using static UGoap.Base.PropertyManager;
using Random = UGoap.Base.Random;

namespace UGoap.Learning
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
            float randomExplore = Random.Next();
            return randomExplore < _learningData.ExploreChance;
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