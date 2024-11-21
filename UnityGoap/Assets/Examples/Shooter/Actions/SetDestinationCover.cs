using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;

namespace UGoap.Unity.Actions
{
    public class SetDestinationCover : GoapAction
    {
        protected override GoapConditions GetProceduralConditions(GoapSettings settings)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapSettings settings)
        {
            return null;
        }
        
        public override bool Validate(ref GoapState goapState, GoapActionInfo actionInfo, IGoapAgent iAgent)
        {
            if (iAgent is not UGoapAgent agent) return false;

            return true;
        }

        public override async Task<GoapState> Execute(GoapState goapState, IGoapAgent iAgent, CancellationToken token)
        {
            if (iAgent is not UGoapAgent agent) return null;

            return goapState;
        }
    }
}