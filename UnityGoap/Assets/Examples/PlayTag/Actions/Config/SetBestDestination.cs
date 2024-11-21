using UGoap.Learning;
using UnityEngine;
using UGoap.Unity.ScriptableObjects;
using UGoap.Unity.Actions;

namespace UGoap.Unity.ActionConfigs
{
    [CreateAssetMenu(fileName = "SetBestDestination", menuName = "Goap Items/Actions/SetBestDestination")]
    public class SetBestDestination : ActionConfig<SetBestDestinationAction>
    {
        public LearningConfig LearningConfig;
        
        protected override SetBestDestinationAction Install(SetBestDestinationAction action)
        {
            action.LearningConfig = LearningConfig;
            return action;
        }
    }
}