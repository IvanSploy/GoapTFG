using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using Action = LUGoap.Base.Action;

namespace LUGoap.Learning
{
    public abstract class LearningAction : Action
    {
        private QLearning _qLearning;
        
        public void SetLearning(QLearning qLearning)
        {
            _qLearning = qLearning;
        }
        
        public int GetLearningCode(State state, Conditions conditions)
        {
            return _qLearning.GetLearningCode(state, conditions);
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
            if (!_qLearning.IsExploring())
            {
                var action = _qLearning.GetBestAction(settings.LocalLearningCode);
                if (action != null) return ParseToParameters(action);
            }
            return OnCreateParameters(settings);
        }

        /// <summary>
        /// Creates the parameters used by the action (should be random).
        /// </summary>
        protected abstract string[] OnCreateParameters(ActionSettings settings);

        public override bool Validate(State nextState, IAgent iAgent, string[] parameters)
        {
            bool valid = OnValidate(nextState, iAgent, parameters);
            if (valid) return true;
            
            _qLearning.Update(iAgent.CurrentAction.GlobalLearningCode,
                ParseToActionName(parameters), _qLearning.FailReward, -1);

            return false;
        }

        public override async Task<Effects> Execute(Effects effects, IAgent iAgent, string[] parameters, CancellationToken token)
        {
            var finalEffects = await OnExecute(effects, iAgent, parameters, token);
            var learningCode = iAgent.CurrentAction.LocalLearningCode;
            
            if(token.IsCancellationRequested)
            {
                _qLearning.Update(learningCode,
                    ParseToActionName(parameters), iAgent.IsCompleted ?
                        _qLearning.SucceedReward : _qLearning.FailReward, iAgent.CurrentAction.LocalLearningCode);
                return null;
            }
            
            _qLearning.Update(learningCode,
                ParseToActionName(parameters), finalEffects != null ?
                    _qLearning.SucceedReward : _qLearning.FailReward, finalEffects != null ? 0 : -1);

            return finalEffects;
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