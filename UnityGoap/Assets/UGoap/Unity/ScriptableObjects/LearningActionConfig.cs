using UGoap.Base;
using UGoap.Learning;
using UnityEngine;

namespace UGoap.Unity.ScriptableObjects
{
    public abstract class LearningActionConfig<TAction> : LearningActionConfig where TAction : LearningAction, new()
    {
        protected override Base.Action CreateActionBase()
        {
            var action = new TAction();
            action.SetLearning(new QLearning(name, QLearningData, _succeedReward, _failReward));
            return Install(action);
        }

        protected abstract TAction Install(TAction action);
    }
    
    public abstract class LearningActionConfig : ActionConfig
    {
        [Header("Main")]
        public QLearningData QLearningData = new()
        {
            Alpha = 0.25f,
            Gamma = 0.9f,
            ExploreRange = new Vector2(-1000,1000),
            ValueRange = 5
        };
        
        [Header("Action")]
        [SerializeField] protected float _succeedReward;
        [SerializeField] protected float _failReward;
    }
}