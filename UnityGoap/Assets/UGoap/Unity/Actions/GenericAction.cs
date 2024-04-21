using UGoap.Base;
using UnityEngine;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "GenericAction", menuName = "Goap Items/Actions/GenericAction", order = 1)]
    public class GenericAction : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private int _waitSeconds = 1;

        protected override GoapConditions GetProceduralConditions(UGoapGoal goal)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(UGoapGoal goal)
        {
            return null;
        }
        
        protected override bool Validate(GoapState goapState)
        {
            return true;
        }

        protected override bool PerformedActions(GoapState goapState, UGoapAgent agent)
        {
            return agent.GoGenericAction(Name, goapState, _waitSeconds);
        }
    }
}