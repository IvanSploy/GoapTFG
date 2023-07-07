using System;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap.Actions
{
    [CreateAssetMenu(fileName = "GoToTarget", menuName = "Goap Items/Actions/GoToTarget", order = 3)]
    public class GoToTargetAction : UGoapAction
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
            
            _target = stateInfo.CurrentGoal[PropertyKey.Target];
            proceduralEffects[PropertyKey.Target] = _target;
            return proceduralEffects;
        }

        protected override void PerformedActions(UGoapAgent agent)
        {
            //GO TO target
            agent.GoToTarget((string)_target);
        }

        public override int GetCost(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var ws = stateInfo.WorldState;
            var goal = stateInfo.CurrentGoal;

            var target1 = ws.Has(PropertyKey.Target) ? (string) ws[PropertyKey.Target] : null;
            var target2 = goal.Has(PropertyKey.Target) ? (string) goal[PropertyKey.Target] : null;

            if (target1 == null || target2 == null) return 999;
            
            var pos1 = UGoapWMM.Get(target1).Position;
            var pos2 = UGoapWMM.Get(target2).Position;

            return Math.Max(5, (int)Vector3.Distance(pos1, pos2));
        }
    }
}