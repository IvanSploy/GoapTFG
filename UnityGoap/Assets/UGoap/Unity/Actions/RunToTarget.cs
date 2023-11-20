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
        private object _target;
        
        protected override bool ProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            if (!stateInfo.WorldState.HasKey(Target)) return true;
            var initialTarget = (string) stateInfo.WorldState[Target]; 
            var finalTarget = (string) stateInfo.CurrentGoal[Target];

            var initialPos = UGoapWMM.Get(initialTarget).Position;
            var finalPos = UGoapWMM.Get(finalTarget).Position;

            return Vector3.Distance(initialPos, finalPos) > 10;
        }
        
        protected override PropertyGroup<PropertyKey, object>
            GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            PropertyGroup<PropertyKey, object> proceduralEffects = new PropertyGroup<PropertyKey, object>();
            
            _target = stateInfo.CurrentGoal[Target];
            proceduralEffects[Target] = _target;
            return proceduralEffects;
        }

        protected override void PerformedActions(UGoapAgent agent)
        {
            //GO TO target
            agent.GoToTargetRunning((string)_target);
        }

        public override int GetCost(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var ws = stateInfo.WorldState;
            var goal = stateInfo.CurrentGoal;

            if (!ws.HasKey(Target) || !goal.Has(Target)) return 50;
            
            var target1 = (string) ws[Target];
            var target2 = (string) goal[Target];

            var pos1 = UGoapWMM.Get(target1).Position;
            var pos2 = UGoapWMM.Get(target2).Position;

            return Math.Max(2, (int)(Vector3.Distance(pos1, pos2) * 0.5f));
        }
    }
}