using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap.Actions
{
    [CreateAssetMenu(fileName = "GoIdle", menuName = "Goap Items/Actions/GoIdle", order = 3)]
    public class GoIdleAction : UGoapActionBase
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
        protected override void PerformedActions(UGoapAgent agent)
        {
            //GO IDLELING
            agent.GoIdleling(agent.speed);
        }

    }
}