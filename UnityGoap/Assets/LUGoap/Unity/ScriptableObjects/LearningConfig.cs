﻿using LUGoap.Learning;
using UnityEngine;
using UnityEngine.Serialization;

namespace LUGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LearningConfig", menuName = "LUGoap/Learning/Config", order = -10)]
    public class LearningConfig : ScriptableObject
    {
        private static readonly string Path = Application.dataPath + "/../Learning/";
        
        [FormerlySerializedAs("QLearningData")] 
        [Header("Main")]
        public QLearningTemplate Learning = new()
        {
            Alpha = 0.25f,
            Gamma = 0.9f,
            ValueRange = 500
        };

        public int MaxExploreValue = 10;

        [Header("Reward")]
        [FormerlySerializedAs("PositiveReward")] public float SuccessReward;
        [FormerlySerializedAs("NegativeReward")] public float FailReward;

        private QLearning _qLearning;
        
        public QLearning GetLearning()
        {
            if (_qLearning == null)
            {
                Load();
            }
            return _qLearning;
        }
        
        [ContextMenu("Load")]
        public void Load()
        {
            _qLearning = new QLearning(Path, name, Learning.DeSerialize(), SuccessReward, FailReward);
            _qLearning.Load();
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
    }
}