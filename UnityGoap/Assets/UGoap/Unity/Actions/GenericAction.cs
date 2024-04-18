using UGoap.Base;
using UnityEngine;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "GenericAction", menuName = "Goap Items/Actions/GenericAction", order = 1)]
    public class GenericAction : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private int _waitSeconds = 1;
        
        protected override bool Validate(GoapStateInfo stateInfo)
        {
            return true;
        }

        protected override GoapConditions GetProceduralConditions(GoapStateInfo stateInfo)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapStateInfo stateInfo)
        {
            return null;
        }

        protected override void PerformedActions(GoapState goapState, UGoapAgent agent)
        {
            agent.GoGenericAction(_waitSeconds);
        }
    }
}