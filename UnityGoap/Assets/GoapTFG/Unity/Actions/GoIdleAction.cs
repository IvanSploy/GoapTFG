using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity.Actions
{
    [CreateAssetMenu(fileName = "GoIdle", menuName = "Goap Items/Actions/GoIdle", order = 3)]
    public class GoIdleAction : GoapActionSO
    {
        protected override bool ProceduralConditions(PropertyGroup<PropertyList, object> worldState)
        {
            return true;
        }

        protected override PropertyGroup<PropertyList, object> ProceduralEffects(PropertyGroup<PropertyList, object> worldState)
        {
            return null;
        }

        protected override void PerformedActions(PropertyGroup<PropertyList, object> worldState)
        {
            //GO IDLELING
        }
    }
}