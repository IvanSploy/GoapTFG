using System.Collections.Generic;
using QGoap.Base;
using QGoap.Learning;
using UnityEngine;
using UnityEngine.Serialization;

namespace QGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LearningConfig", menuName = "QGoap/Learning/Config", order = -10)]
    public class LearningConfig : ScriptableObject
    {
        private static readonly string Path = Application.dataPath + "/../Learning/";
        
        [Header("Main")]
        [Range(0f,1f)] public float Alpha;
        [Range(0f,1f)] public float Gamma;
        
        [Header("Exploration")]
        [Range(0f,1f)] public float Epsilon;

        [Header("Filtering")]
        public List<PropertyManager.PropertyKey> FilterKeys;
        public List<PropertyManager.PropertyKey> AdditionalKeys;
        public int ValueRange;

        public int MaxExploreValue = 10;

        [Header("Reward")]
        [FormerlySerializedAs("PositiveReward")] public float SuccessReward;
        [FormerlySerializedAs("NegativeReward")] public float FailReward;

        private QLearning _learning;
        
        public QLearning GetLearning()
        {
            if (_learning == null)
            {
                Load();
            }
            return _learning;
        }
        
        [ContextMenu("Load")]
        public void Load()
        {
            _learning = new QLearning(Path, name, GetLearningConfig(), SuccessReward, FailReward);
            _learning.Load();
        }

        [ContextMenu("Save")]
        public void Save()
        {
            var qLearning = GetLearning();
            qLearning.Save();
        }

        [ContextMenu("Clear Data")]
        public void Clear()
        {
            var qLearning = GetLearning();
            qLearning.Clear();
            qLearning.Save();
        }
        
        public QLearningConfig GetLearningConfig()
        {
            return new QLearningConfig
            {
                Alpha = Alpha,
                Gamma = Gamma,
                Epsilon = Epsilon,
                FilterKeys = FilterKeys,
                AdditionalKeys = AdditionalKeys,
                ValueRange = ValueRange
            };
        }
    }
}