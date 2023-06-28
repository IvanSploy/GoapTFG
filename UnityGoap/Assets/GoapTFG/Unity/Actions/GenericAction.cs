using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity.Actions
{
    [CreateAssetMenu(fileName = "GenericAction", menuName = "Goap Items/Actions/GenericAction", order = 1)]
    public class GenericAction : BaseGoapAction
    {
        protected override bool ProceduralConditions(GoapStateInfo<PropertyList, object> stateInfo)
        {
            return true;
        }

        protected override PropertyGroup<PropertyList, object> GetProceduralEffects(GoapStateInfo<PropertyList, object> stateInfo)
        {
            return null;
        }

        protected override HashSet<PropertyList> GetAffectedPropertyLists()
        {
            return null;
        }

        protected override void PerformedActions(GoapAgent goapAgent) { }
    }
}