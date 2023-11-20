using System;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;
using static GoapTFG.UGoap.UGoapPropertyManager.PropertyKey;

namespace GoapTFG.UGoap.Actions
{
    [CreateAssetMenu(fileName = "WalkToTarget", menuName = "Goap Items/Actions/WalkToTarget", order = 3)]
    public class WalkToTarget : UGoapAction
    {
        private object _target;
        
        protected override bool ProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        { 
            return true;
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
            agent.GoToTargetWalking((string)_target);
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

            return Math.Max(5, (int)(Vector3.Distance(pos1, pos2)));
        }
    }
}