using UGoap.Base;
using UnityEngine;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "SetDestinationCover", menuName = "Goap Items/Actions/Shooter/SetDestinationCover")]
    public class SetDestinationCover : UGoapAction
    {
        protected override GoapConditions GetProceduralConditions(GoapSettings settings)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapSettings settings)
        {
            return null;
        }
        
        public override bool ProceduralValidate(GoapState goapState, GoapActionInfo actionInfo, UGoapAgent agent)
        {
            return agent.ValidateGeneric(Name, goapState);
        }

        public override void ProceduralExecute(ref GoapState goapState, UGoapAgent agent)
        {
            agent.GoGenericAction(Name, ref goapState);
        }
    }
}