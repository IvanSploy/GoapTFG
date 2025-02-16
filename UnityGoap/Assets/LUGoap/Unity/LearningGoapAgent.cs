using LUGoap.Learning;
using LUGoap.Planning;
using LUGoap.Unity.ScriptableObjects;
using UnityEngine;

namespace LUGoap.Unity
{
    public class LearningGoapAgent : GoapAgent, ILearningAgent
    {
        [Header("Learning")]
        [SerializeField] private LearningConfig _learningConfig;
        
        public QLearning Learning { get; private set; }

        protected override Planner CreatePlanner()
        {
            if (!_learningConfig) return base.CreatePlanner();
            
            Learning = _learningConfig.GetLearning();
            var generator = new AStar();
            generator.SetLearning(Learning, _learningConfig.MaxExploreValue);
            return new BackwardPlanner(generator, this);
        }

        public new void OnDestroy()
        {
            _learningConfig?.Save();
            base.OnDestroy();
        }
    }
}