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

        public QLearning Create()
        {
            return new QLearning(name, QLearningData, PositiveReward, NegativeReward);
        }
    }
}