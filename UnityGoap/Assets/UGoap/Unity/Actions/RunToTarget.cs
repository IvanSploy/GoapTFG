using System;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;
using static GoapTFG.UGoap.UGoapPropertyManager.PropertyKey;

namespace GoapTFG.UGoap.Actions
{
    [CreateAssetMenu(fileName = "RunToTarget", menuName = "Goap Items/Actions/RunToTarget", order = 3)]
    public class RunToTarget : UGoapAction
    {
        protected override bool ProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            if (!stateInfo.State.HasKey(Target)) return true;
            var initialTarget = (string) stateInfo.State[Target]; 
            var finalTarget = (string) stateInfo.Goal[Target];

            var initialPos = UGoapWMM.Get(initialTarget).Position;
            var finalPos = UGoapWMM.Get(finalTarget).Position;

            return Vector3.Distance(initialPos, finalPos) > 10;
        }
        
        protected override PropertyGroup<PropertyKey, object>
            GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            PropertyGroup<PropertyKey, object> proceduralEffects = new PropertyGroup<PropertyKey, object>();
            
            proceduralEffects[Target] = stateInfo.Goal[Target];
            return proceduralEffects;
        }

        protected override void PerformedActions(PropertyGroup<PropertyKey, object> proceduralEffects, UGoapAgent agent)
        {
            //GO TO target
            if (proceduralEffects.HasKey(Target))
            {
                agent.GoToTargetRunning((string)proceduralEffects[Target]);
            }
        }

        public override int GetCost(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var ws = stateInfo.State;
            var goal = stateInfo.Goal;

            if (!ws.HasKey(Target) || !goal.Has(Target)) return 50;
            
            var target1 = (string) ws[Target];
            var target2 = (string) goal[Target];

            var pos1 = UGoapWMM.Get(target1).Position;
            var pos2 = UGoapWMM.Get(target2).Position;

            return Math.Max(2, (int)(Vector3.Distance(pos1, pos2) * 0.5f));
        }
    }
}