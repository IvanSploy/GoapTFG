using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity.ScriptableObjects;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "Standard", menuName = "UGoap/Standard Action")]
    public class Standard : ActionConfig<StandardAction>
    {
        public int WaitSeconds = 1;
        
        protected override StandardAction Install(StandardAction action)
        {
            action.WaitSeconds = WaitSeconds;
            return action;
        }
    }
    
    public class StandardAction : Base.Action
    {
        public int WaitSeconds = 1;
        
        protected override Conditions GetProceduralConditions(ActionSettings settings)
        {
            return null;
        }

        protected override Effects GetProceduralEffects(ActionSettings settings)
        {
            return null;
        }
        
        protected override bool OnValidate(State nextState, IAgent iAgent, string[] parameters)
        {
            if (iAgent is not UGoapAgent agent) return false;
            
            return true;
        }

        protected override async Task<State> OnExecute(State state, IAgent iAgent, string[] parameters, CancellationToken token)
        {
            await Task.Delay(WaitSeconds * 1000, token);
            return state;
        }
    }
}