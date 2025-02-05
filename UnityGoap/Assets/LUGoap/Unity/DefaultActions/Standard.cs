using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;

namespace LUGoap.Unity.Actions
{
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