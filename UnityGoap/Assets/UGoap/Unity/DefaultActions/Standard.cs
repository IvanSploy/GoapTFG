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
    
    public class StandardAction : GoapAction
    {
        public int WaitSeconds = 1;
        
        protected override GoapConditions GetProceduralConditions(GoapSettings settings)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapSettings settings)
        {
            return null;
        }
        
        public override bool Validate(GoapState goapState, GoapActionInfo actionInfo, IGoapAgent iAgent)
        {
            return true;
        }

        public override async Task<GoapState> Execute(GoapState goapState, IGoapAgent iAgent, CancellationToken token)
        {
            await Task.Delay(WaitSeconds * 1000, token);
            return goapState;
        }
    }
}