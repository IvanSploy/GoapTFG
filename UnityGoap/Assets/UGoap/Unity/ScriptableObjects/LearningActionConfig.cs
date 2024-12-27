using UGoap.Learning;
using UnityEngine;

namespace UGoap.Unity.ScriptableObjects
{
    public abstract class LearningActionConfig<TAction> : LearningActionConfig where TAction : LearningAction, new()
    {
        protected override Base.Action CreateActionBase()
        {
            var action = new TAction();
            action.SetLearning(GetLearning());
            return Install(action);
        }

        protected abstract TAction Install(TAction action);
    }
    
    public abstract class LearningActionConfig : ActionConfig
    {
        private static readonly string Path = Application.dataPath + "/../Learning/" + "ActionLearning/";
        
        public QLearningTemplate LearningData = new()
        {
            Alpha = 0.25f,
            Gamma = 0.9f,
            ValueRange = 5
        };
        
        [Header("Action")]
        [SerializeField] protected float _succeedReward;
        [SerializeField] protected float _failReward;
        
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
            _qLearning = new QLearning(Path, name, LearningData.DeSerialize(), _succeedReward, _failReward);
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