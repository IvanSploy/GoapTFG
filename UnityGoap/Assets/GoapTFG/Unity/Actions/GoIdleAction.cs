using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity.Actions
{
    [CreateAssetMenu(fileName = "GoIdle", menuName = "Goap Items/Actions/GoIdle", order = 3)]
    public class GoIdleAction : GoapActionSO
    {
        protected override bool ProceduralConditions(GoapStateInfo<PropertyList, object> stateInfo)
        {
            return true;
        }

        protected override PropertyGroup<PropertyList, object> GetProceduralEffects(GoapStateInfo<PropertyList, object> stateInfo)
        {
            return null;
        }

        protected override void PerformedActions(GoapAgent goapAgent)
        {
            //GO IDLELING
            goapAgent.GoIdleling(goapAgent.speed);
        }

    }
}