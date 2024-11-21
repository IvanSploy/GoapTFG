using UnityEngine;
using UGoap.Unity.ScriptableObjects;
using UGoap.Unity.Actions;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.Action.Configs
{
    [CreateAssetMenu(fileName = "GetResource", menuName = "Goap Items/Actions/GetResource")]
    public class GetResource : ActionConfig<GetResourceAction>
    {
        [Header("Custom Data")]
        public PropertyKey Resource;
        public float Count = 1;
        public int WaitSeconds = 1;
        
        protected override GetResourceAction Install(GetResourceAction action)
        {
            action.Resource = Resource;
            action.Count = Count;
            action.WaitSeconds = WaitSeconds;
            return action;
        }
    }
}