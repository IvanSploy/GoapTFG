using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity;
using static UGoap.Base.UGoapPropertyManager;

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
        
        public override bool Validate(ref GoapState goapState, GoapActionInfo actionInfo, IGoapAgent iAgent)
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