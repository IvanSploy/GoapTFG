/*using UGoap.Base;
using UnityEngine;

namespace UGoap.Unity.Actions
{
    public class MainAction : GoapAction
    {
        private float WaitSeconds;
        
        public MainAction(string name, GoapConditions conditions, GoapEffects effects, float waitSeconds = 1f) : base(name, conditions, effects)
        {
            WaitSeconds = waitSeconds;
        }

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
            UGoapAgent uAgent = agent as UGoapAgent;
            if (uAgent)
            {
                return uAgent.ValidateGeneric(Name, goapState);
            }

            return true;
        }

        public override void Execute(ref GoapState goapState, IGoapAgent agent)
        {
            UGoapAgent uAgent = agent as UGoapAgent;
            if (uAgent)
            {
                uAgent.GoGenericAction(Name, ref goapState, WaitSeconds);
            }
        }
    }
}*/