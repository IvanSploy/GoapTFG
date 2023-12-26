using UGoap.Base;
using UnityEngine;
using static UGoap.Unity.UGoapPropertyManager;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "GenericAction", menuName = "Goap Items/Actions/GenericAction", order = 1)]
    public class GenericAction : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private int _waitSeconds = 1;
        
        protected override bool Validate(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return true;
        }

        protected override ConditionGroup<PropertyKey, object> GetProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return null;
        }

        protected override EffectGroup<PropertyKey, object> GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return null;
        }

        protected override void PerformedActions(PropertyGroup<PropertyKey, object> state, UGoapAgent agent)
        {
            agent.GoGenericAction(_waitSeconds);
        }
    }
}