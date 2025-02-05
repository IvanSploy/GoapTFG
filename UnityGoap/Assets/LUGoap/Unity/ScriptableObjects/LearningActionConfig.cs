using LUGoap.Learning;
using UnityEngine;
using UnityEngine.Serialization;

namespace LUGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LearningAction", menuName = "LUGoap/Learning/Action")]
    public class LearningActionConfig : ActionBaseConfig
    {
        private static readonly string Path = Application.dataPath + "/../Learning/" + "ActionLearning/";
        
        [FormerlySerializedAs("LearningData")]
        [SerializeField] private QLearningTemplate _learningData = new()
        {
            Alpha = 0.25f,
            Gamma = 0.9f,
            ValueRange = 5
        };
        
        [SerializeField] protected float _succeedReward;
        [SerializeField] protected float _failReward;
        
        [SerializeReference] private LearningAction _actionData;
        
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
            _qLearning = new QLearning(Path, name, _learningData.DeSerialize(), _succeedReward, _failReward);
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

        protected override Base.Action CreateAction()
        {
            _actionData.SetLearning(GetLearning());
            return _actionData;
        }
    }
}