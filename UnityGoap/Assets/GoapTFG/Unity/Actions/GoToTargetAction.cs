using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity.Actions
{
    [CreateAssetMenu(fileName = "GoToTarget", menuName = "Goap Items/Actions/GoToTarget", order = 3)]
    public class GoToTargetAction : GoapActionSO
    {
        private object _target;
        
        protected override bool ProceduralConditions(GoapStateInfo<PropertyList, object> stateInfo)
        {
            return true;
        }

        protected override PropertyGroup<PropertyList, object> GetProceduralEffects(GoapStateInfo<PropertyList, object> stateInfo)
        {
            PropertyGroup<PropertyList, object> proceduralEffects = new PropertyGroup<PropertyList, object>();
            var goal = stateInfo.CurrentGoal;
            if (goal.Has(PropertyList.Target))
            {
                _target = goal[PropertyList.Target];
                proceduralEffects[PropertyList.Target] = _target;
                return proceduralEffects;
            }
            return null;
        }
        
        protected override void PerformedActions(GoapAgent goapAgent)
        {
            //GO TO target
            goapAgent.GoToTarget((string)_target);
        }
    }
}