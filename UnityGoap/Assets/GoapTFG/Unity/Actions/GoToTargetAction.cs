using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity.Actions
{
    [CreateAssetMenu(fileName = "GoToTarget", menuName = "Goap Items/Actions/GoToTarget", order = 3)]
    public class GoToTargetAction : GoapActionSO
    {
        protected override bool ProceduralConditions(PropertyGroup<PropertyList, object> worldState)
        {
            return true;
        }

        protected override PropertyGroup<PropertyList, object> ProceduralEffects(PropertyGroup<PropertyList, object> worldState)
        {
            return null;
        }
        
        public override bool CheckCustomParameters(GoapGoal<PropertyList, object> currentGoal)
        {
            currentGoal[PropertyList.Target].Equals("");
            return true;
        }
        
        protected override void PerformedActions(PropertyGroup<PropertyList, object> worldState)
        {
            //GO TO target
            
        }
    }
}