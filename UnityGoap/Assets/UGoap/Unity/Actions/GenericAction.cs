using System.Threading.Tasks;
using UGoap.Base;

namespace UGoap.Unity.Actions
{
    public class GenericAction : GoapAction
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
        
        public override bool Validate(GoapState goapState, GoapActionInfo actionInfo, IGoapAgent agent)
        {
            return true;
        }

        public override async Task<GoapState> Execute(GoapState goapState, IGoapAgent agent)
        {
            await Task.Delay(WaitSeconds * 1000);
            return goapState;
        }
    }
}