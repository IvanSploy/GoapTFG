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
        
        public override bool ProceduralValidate(GoapState goapState, UGoapAgent agent)
        {
            return agent.ValidateGeneric(Name, goapState);
        }

        public override void ProceduralExecute(GoapState goapState, UGoapAgent agent)
        {
            agent.GoGenericAction(Name, goapState, _waitSeconds);
        }
    }
}