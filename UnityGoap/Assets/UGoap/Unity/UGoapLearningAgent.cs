using UGoap.Learning;
using UGoap.Planning;
using UGoap.Unity.ScriptableObjects;
using UnityEngine;

namespace UGoap.Unity
{
    public class UGoapLearningAgent : UGoapAgent, ILearningAgent
    {
        [Header("Learning")]
        [SerializeField] private LearningConfig _learningConfig;
        
        public QLearning Learning { get; private set; }

        protected override Planner CreatePlanner()
        {
            if (!_learningConfig) return base.CreatePlanner();
            
            Learning = _learningConfig.Create();
            var generator = new AStar();
            generator.SetLearning(Learning);
            return new BackwardPlanner(generator, this);
        }

        public new void OnDestroy()
        {
            _learningConfig?.Save();
            base.OnDestroy();
        }
    }
}