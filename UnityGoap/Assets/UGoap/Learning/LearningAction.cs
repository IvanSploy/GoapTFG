using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using Action = UGoap.Base.Action;

namespace UGoap.Learning
{
    public abstract class LearningAction : Action
    {
        private QLearning _qLearning;
        
        public void SetLearning(QLearning qLearning)
        {
            _qLearning = qLearning;
        }

        public override string[] CreateParameters(State state, Conditions conditions)
        {
            if (!_qLearning.IsExploring())
            {
                var action = _qLearning.GetBestAction(state, conditions);
                if (action != null) return ParseToParameters(action);
            }
            return OnCreateParameters();
        }

        /// <summary>
        /// Creates the parameters used by the action (should be random).
        /// </summary>
        protected abstract string[] OnCreateParameters();

        public override bool Validate(State nextState, IAgent iAgent, string[] parameters)
        {
            var initialState = iAgent.CurrentState;
            bool valid = OnValidate(nextState, iAgent, parameters);
            if (valid) return true;

            _qLearning.Update(iAgent.CurrentGoal.Conditions, initialState,
                ParseToActionName(parameters), _qLearning.FailReward, iAgent.CurrentState);

            return false;
        }

        public override async Task<Effects> Execute(Effects effects, IAgent iAgent, string[] parameters, CancellationToken token)
        {
            var initialState = iAgent.CurrentState;
            var finalEffects = await OnExecute(effects, iAgent, parameters, token);
            
            if(token.IsCancellationRequested)
            {
                _qLearning.Update(iAgent.CurrentGoal.Conditions, initialState,
                    ParseToActionName(parameters), iAgent.IsCompleted ?
                        _qLearning.SucceedReward : _qLearning.FailReward, iAgent.CurrentState);
                return null;
            }
            
            var finalState = iAgent.CurrentState;
            if(finalEffects != null) finalState += finalEffects;
            
            _qLearning.Update(iAgent.CurrentGoal.Conditions, initialState,
                ParseToActionName(parameters), finalEffects != null ?
                    _qLearning.SucceedReward : _qLearning.FailReward, finalState ?? iAgent.CurrentState);

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