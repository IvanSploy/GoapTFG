using System.Collections.Generic;
using QGoap.Base;
using QGoap.Learning;
using UnityEngine;
using UnityEngine.Serialization;

namespace QGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "LearningAction", menuName = "LUGoap/Learning/Action")]
    public class LearningActionConfig : ActionBaseConfig
    {
        private static readonly string Path = Application.dataPath + "/../Learning/" + "ActionLearning/";

        [FormerlySerializedAs("LearningData")] [SerializeField]
        private QLearningTemplate _learningData = QLearningTemplate.GetDefault();
        
        [SerializeField] protected float _succeedReward;
        [SerializeField] protected float _failReward;
        
        [SerializeReference] private LearningAction _actionData;
        
        private QLearning _localLearning;
        
        public QLearning GetLearning()
        {
            if (_localLearning == null)
            {
                Load();
            }
            return _localLearning;
        }
        
        [ContextMenu("Load")]
        public void Load()
        {
            _localLearning = new QLearning(Path, name, _learningData.DeSerialize(), _succeedReward, _failReward);
            _localLearning.Load();
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