using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap.Actions
{
    [CreateAssetMenu(fileName = "GenericAction", menuName = "Goap Items/Actions/GenericAction", order = 1)]
    public class GenericAction : UGoapActionBase
    {
        protected override bool ProceduralConditions(GoapStateInfo<UGoapPropertyManager.PropertyList, object> stateInfo)
        {
            return true;
        }

        protected override PropertyGroup<UGoapPropertyManager.PropertyList, object> GetProceduralEffects(GoapStateInfo<UGoapPropertyManager.PropertyList, object> stateInfo)
        {
            return null;
        }

        protected override HashSet<UGoapPropertyManager.PropertyList> GetAffectedPropertyLists()
        {
            return null;
        }

        protected override void PerformedActions(UGoapAgent agent) { }
    }
}