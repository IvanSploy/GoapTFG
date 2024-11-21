using UnityEngine;
using UGoap.Unity.ScriptableObjects;
using UGoap.Unity.Actions;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.Action.Configs
{
    [CreateAssetMenu(fileName = "GoTo", menuName = "Goap Items/Actions/GoTo")]
    public class GoToTarget : ActionConfig<GoToTargetAction>
    {
        [Header("Custom Data")]
        public PropertyKey TargetKey;
        public int SpeedFactor = 1;
        public string ExcludedLocation = "none";
        
        protected override GoToTargetAction Install(GoToTargetAction targetAction)
        {
            targetAction.TargetKey = TargetKey;
            targetAction.SpeedFactor = SpeedFactor;
            targetAction.ExcludedLocation = ExcludedLocation;
            return targetAction;
        }
    }
}