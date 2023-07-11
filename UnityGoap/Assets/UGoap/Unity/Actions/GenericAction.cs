using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap.Actions
{
    [CreateAssetMenu(fileName = "GenericAction", menuName = "Goap Items/Actions/GenericAction", order = 1)]
    public class GenericAction : UGoapAction
    {
        protected override bool ProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return true;
        }

        protected override PropertyGroup<PropertyKey, object> GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return null;
        }

        protected override void PerformedActions(UGoapAgent agent)
        {
            agent.GoGenericAction(GetCost());
        }
    }
}