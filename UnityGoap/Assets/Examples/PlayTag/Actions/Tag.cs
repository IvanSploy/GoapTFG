using System.Threading;
using UGoap.Base;
using System.Threading.Tasks;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.Actions
{
    public class TagAction : GoapAction
    {
        
        
        protected override GoapConditions GetProceduralConditions(GoapSettings settings)
        {
            return null;
        }
        
        protected override GoapEffects GetProceduralEffects(GoapSettings settings)
        {
            return null;
        }
        
        public override bool Validate(ref GoapState state, GoapActionInfo actionInfo, IGoapAgent iAgent)
        {
            if (iAgent is not UGoapAgent agent) return false;
            
            var isIt = state.TryGetOrDefault(PropertyKey.IsIt, true);
            if (isIt)
            {
                state.Set(PropertyKey.MoveState, "Ready");
                return false;
            }
            
            return true;
        }
        
        public override async Task<GoapState> Execute(GoapState state, IGoapAgent iAgent, CancellationToken token)
        {
            if (iAgent is not UGoapAgent agent) return null;
            return state;
        }
    }
}