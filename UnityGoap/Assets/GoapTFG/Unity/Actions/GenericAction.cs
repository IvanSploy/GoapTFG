using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity.Actions
{
    [CreateAssetMenu(fileName = "GenericAction", menuName = "Goap Items/Actions/GenericAction", order = 1)]
    public class GenericAction : GoapActionSO
    {
        protected override bool 
            ProceduralConditions(PropertyGroup<PropertyList, object> worldState) => true;

        protected override PropertyGroup<PropertyList, object>
            ProceduralEffects(PropertyGroup<PropertyList, object> worldState) => null;

        protected override void PerformedActions(PropertyGroup<PropertyList, object> worldState) { }
        public override bool CheckCustomParameters(GoapGoal<PropertyList, object> currentGoal) => true;
    }
}