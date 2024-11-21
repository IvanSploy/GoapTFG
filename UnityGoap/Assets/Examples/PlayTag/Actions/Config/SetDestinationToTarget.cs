using UnityEngine;
using UGoap.Unity.ScriptableObjects;
using UGoap.Unity.Actions;

namespace UGoap.Unity.ActionConfigs
{
    [CreateAssetMenu(fileName = "SetDestinationToTarget", menuName = "Goap Items/Actions/SetDestinationToTarget")]
    public class SetDestinationToTarget : ActionConfig<SetDestinationToTargetAction>
    {
        public string Target = "None";
        protected override SetDestinationToTargetAction Install(SetDestinationToTargetAction action)
        {
            action.Target = Target;
            return action;
        }
    }
}