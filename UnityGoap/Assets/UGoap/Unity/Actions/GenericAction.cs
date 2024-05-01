using UGoap.Base;
using UnityEngine;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "GenericAction", menuName = "Goap Items/Actions/GenericAction", order = 1)]
    public class GenericAction : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private int _waitSeconds = 1;

        protected override GoapConditions GetProceduralConditions(GoapConditions goal)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapConditions goal)
        {
            return null;
        }
        
        public override bool Validate(GoapState goapState, IGoapAgent agent)
        {
            UGoapAgent uAgent = agent as UGoapAgent;
            if (uAgent)
            {
                return uAgent.ValidateGeneric(Name, goapState);
            }

            return true;
        }

        public override void PerformedActions(GoapState goapState, IGoapAgent agent)
        {
            UGoapAgent uAgent = agent as UGoapAgent;
            if (uAgent)
            {
                uAgent.GoGenericAction(Name, goapState, _waitSeconds);
            }
        }
    }
}