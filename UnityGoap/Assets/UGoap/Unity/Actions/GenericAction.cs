using UGoap.Base;
using UnityEngine;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "GenericAction", menuName = "Goap Items/Actions/GenericAction", order = 0)]
    public class GenericAction : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private int _waitSeconds = 1;

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
            agent.GoGenericAction(Name, ref goapState, _waitSeconds);
        }
    }
}