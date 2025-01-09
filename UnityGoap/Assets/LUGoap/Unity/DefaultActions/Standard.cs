using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity.ScriptableObjects;

namespace LUGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "Standard", menuName = "LUGoap/Actions/Default Action", order = -1)]
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
            if (iAgent is not GoapAgent agent) return false;
            
            return true;
        }

        protected override async Task<Effects> OnExecute(Effects effects, IAgent iAgent, string[] parameters, CancellationToken token)
        {
            await Task.Delay(WaitSeconds * 1000, token);
            return effects;
        }
    }
}