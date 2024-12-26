using UGoap.Learning;
using UnityEngine;

namespace UGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LearningConfig", menuName = "UGoap/LearningConfig", order = 1)]
    public class LearningConfig : ScriptableObject
    {
        [Header("Main")]
        public QLearningData QLearningData = new()
        {
            Alpha = 0.25f,
            Gamma = 0.9f,
            ExploreRange = new Vector2(-1000,1000),
            ValueRange = 500
        };

        [Header("Reward")]
        public float PositiveReward;
        public float NegativeReward;

        private QLearning _qLearning;
        
        public QLearning Create()
        {
            _qLearning ??= new QLearning(name, QLearningData, PositiveReward, NegativeReward);
            _qLearning.Load();
            return _qLearning;
        }
        
        [ContextMenu("Load")]
        public void Load()
        {
            Create();
        }

        [ContextMenu("Save")]
        public void Save()
        {
            Create();
            _qLearning.Save();
        }

        [ContextMenu("Clear Data")]
        public void Clear()
        {
            Create();
            _qLearning.Clear();
            _qLearning.Save();
        }
    }
}