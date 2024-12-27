using UGoap.Learning;
using UnityEngine;
using UnityEngine.Serialization;

namespace UGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LearningConfig", menuName = "UGoap/LearningConfig", order = 1)]
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

        [SerializeField] private Vector2 _exploreRange = new(-1000,1000);
        public System.Numerics.Vector2 ExploreRange => new(_exploreRange.x, _exploreRange.y);

        [Header("Reward")]
        public float PositiveReward;
        public float NegativeReward;

        private QLearning _qLearning;
        
        
        public QLearning Create()
        {
            _qLearning ??= new QLearning(Path, name, Learning.DeSerialize(), PositiveReward, NegativeReward);
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