using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap.Actions
{
    [CreateAssetMenu(fileName = "GoIdle", menuName = "Goap Items/Actions/GoIdle", order = 3)]
    public class GoIdleAction : UGoapAction
    {
        protected override bool ProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return true;
        }

        protected override PropertyGroup<PropertyKey, object> GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return null;
        }

        protected override HashSet<PropertyKey> GetAffectedPropertyKeys()
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