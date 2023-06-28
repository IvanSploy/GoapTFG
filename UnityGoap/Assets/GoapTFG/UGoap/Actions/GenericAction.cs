using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap.Actions
{
    [CreateAssetMenu(fileName = "GenericAction", menuName = "Goap Items/Actions/GenericAction", order = 1)]
    public class GenericAction : UGoapActionBase
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

        protected override void PerformedActions(UGoapAgent agent) { }
    }
}