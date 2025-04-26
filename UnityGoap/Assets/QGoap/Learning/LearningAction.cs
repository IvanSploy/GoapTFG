using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;
using Action = QGoap.Base.Action;
using Base_Action = QGoap.Base.Action;

namespace QGoap.Learning
{
    [Serializable]
    public abstract class LearningAction : Base_Action
    {
        private QLearning _learning;
        
        public void SetLearning(QLearning learning)
        {
            _learning = learning;
        }
        
        public int GetLearningCode(State state, ConditionGroup conditionGroup)
        {
            return _learning.GetLearningCode(state, conditionGroup);
        }
        
        public override ActionSettings CreateSettings(ActionSettings settings)
        {
            var learningCode = GetLearningCode(settings.InitialState, settings.Goal);
            settings.LocalLearningCode = learningCode;
            var parameters = CreateParameters(settings);
            settings.Parameters = parameters;
            return settings;
        }

        public string[] CreateParameters(ActionSettings settings)
        {
            if (!_learning.IsExploring())
            {
                var action = _learning.GetBestAction(settings.LocalLearningCode);
                if (action != null) return ParseToParameters(action);
            }
            return OnCreateParameters(settings);
        }

        /// <summary>
        /// Creates the parameters used by the action (should be random).
        /// </summary>
        protected abstract string[] OnCreateParameters(ActionSettings settings);

        public override bool Validate(State nextState, string[] parameters)
        {
            bool valid = OnValidate(nextState, parameters);
            if (valid) return true;
            
            _learning.Update(_agent.CurrentAction.GlobalLearningCode,
                ParseToActionName(parameters), _learning.FailReward);

            return false;
        }

        public override async Task<EffectGroup> Execute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
        {
            EffectGroup finalEffectGroup;
            var learningCode = _agent.CurrentAction.LocalLearningCode;
            try
            {
                finalEffectGroup = await OnExecute(effectGroup, parameters, token);
            }
            catch (OperationCanceledException)
            {
                goto OnFail;
            }

            if (!token.IsCancellationRequested)
            {
                ApplyReward(learningCode, parameters,
                    finalEffectGroup != null ? _learning.SucceedReward : _learning.FailReward);
                return finalEffectGroup;
            }

            OnFail:
            ApplyReward(learningCode, parameters, _agent.IsCompleted ? _learning.SucceedReward : _learning.FailReward);
            
            return null;
        }

        public void ApplyReward(int learningCode, string[] parameters, float reward)
        {
            _learning.Update(learningCode,
                ParseToActionName(parameters),
                reward);
        }

        private string[] ParseToParameters(string actionName)
        {
            return actionName.Split('_').Skip(1).ToArray();
        }

        private string ParseToActionName(string[] parameters)
        {
            return parameters.Aggregate(Name, (current, param) => $"{current}_{param}");
        }
    }
}