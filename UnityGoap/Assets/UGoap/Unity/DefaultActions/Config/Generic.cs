using UnityEngine;
using UGoap.Unity.Actions;
using UGoap.Unity.ScriptableObjects;

namespace UGoap.Unity.Action.Configs
{
    [CreateAssetMenu(fileName = "Generic", menuName = "Goap Items/Actions/Generic")]
    public class Generic : ActionConfig<GenericAction>
    {
        public int WaitSeconds = 1;
        
        protected override GenericAction Install(GenericAction action)
        {
            action.WaitSeconds = WaitSeconds;
            return action;
        }
    }
}