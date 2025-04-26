using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using QGoap.Base;
using Unity.Plastic.Newtonsoft.Json;
using Random = QGoap.Base.Random;

namespace QGoap.Learning
{
    public class QLearning
    {
        [Serializable]
        private class QLearningData
        {
            public Dictionary<int, Dictionary<string, float>> Values;
            public float MaxValue;
        }
        
        private readonly string _path;
        private readonly string _file;
        
        private Dictionary<int, Dictionary<string, float>> _qValues;
        public float MaxValue;
        
        private readonly Func<Dictionary<string, float>, string> _bestActionPolitic;
        
        private readonly QLearningConfig _learningConfig;
        public float SucceedReward { get; private set; }
        public float FailReward { get; private set; }

        public QLearning(string dataPath, string fileName, QLearningConfig config, float succeedReward, float failReward,
            Func<Dictionary<string, float>, string> bestActionPolitic = null)
        {
            _path = dataPath;
            _file = fileName + ".json";
            _learningConfig = config;
            SucceedReward = succeedReward;
            FailReward = failReward;
            _bestActionPolitic = bestActionPolitic ?? Politics.GetMax;
        }

        public void Load()
        {
            CreateFile();
            var fullPath = Path.Combine(_path, _file);
            var text = File.ReadAllText(fullPath);
            var info = JsonConvert.DeserializeObject<QLearningData>(text);
            if (info is { Values: not null })
            {
                _qValues = info.Values;
                MaxValue = info.MaxValue;
            }
            else
            {
                _qValues = JsonConvert.DeserializeObject<Dictionary<int, Dictionary<string, float>>>(text);
                MaxValue = int.MinValue;
                if (_qValues != null)
                {
                    foreach (var pair in _qValues)
                    foreach (var actionValue in pair.Value)
                    {
                        if (MaxValue < actionValue.Value)
                        {
                            MaxValue = actionValue.Value;
                        }
                    }
                }
                else
                {
                    _qValues = new Dictionary<int, Dictionary<string, float>>();
                }
            }
        }

        public void Save()
        {
            CreateFile();
            var fullPath = Path.Combine(_path, _file);
            var info = new QLearningData()
            {
                Values = _qValues,
                MaxValue = MaxValue
            };
            File.WriteAllText(fullPath, JsonConvert.SerializeObject(info, Formatting.Indented));
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
        
        public int GetLearningCode(State state, ConditionGroup conditionGroup)
        {
            var distances = conditionGroup.GetDistances(state, _learningConfig.FilterKeys, _learningConfig.AdditionalKeys);
            if(distances.Count == 0) return 0;
            
            if (_learningConfig.ValueRange > 1)
            {
                foreach (var pair in distances.ToList())
                {
                    distances[pair.Key] = pair.Value / _learningConfig.ValueRange + 1;
                }
            }

            var list = distances.ToList();
            list.Sort((comp1, comp2) => comp1.Key.CompareTo(comp2.Key));

            int hash = 17;
            foreach(KeyValuePair<PropertyManager.PropertyKey, int> kvp in list)
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
        
        public void Update(int learningCode, string action, float r, int nextLearningCode = 0)
        {
            var qValue = (1 - _learningConfig.Alpha) * Get(learningCode, action) + _learningConfig.Alpha * (r + _learningConfig.Gamma * GetMax(nextLearningCode));
            Set(learningCode, action, qValue);
        }
        
        //Exploration
        public bool IsExploring()
        {
            float exploreChance = Random.Next();
            return exploreChance < _learningConfig.Epsilon;
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
            
            if (MaxValue < _qValues[state][action])
            {
                MaxValue = _qValues[state][action];
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