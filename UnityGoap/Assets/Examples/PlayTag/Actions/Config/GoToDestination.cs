using UnityEngine;
using UGoap.Unity.Actions;
using UGoap.Unity.ScriptableObjects;

namespace UGoap.Unity.ConfigActions
{
    [CreateAssetMenu(fileName = "GoToDestination", menuName = "Goap Items/Actions/GoToDestination")]
    public class GoToDestination : ActionConfig<GoToDestinationAction>
    {
        public int SpeedFactor = 1;
        protected override GoToDestinationAction Install(GoToDestinationAction action)
        {
            action.SpeedFactor = SpeedFactor;
            return action;
        }
    }
}