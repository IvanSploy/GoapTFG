using System;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap.Actions
{
    [CreateAssetMenu(fileName = "GoToTarget", menuName = "Goap Items/Actions/GoToTarget", order = 3)]
    public class GoToTargetAction : UGoapActionBase
    {
        private object _target;
        
        protected override bool ProceduralConditions(GoapStateInfo<UGoapPropertyManager.PropertyList, object> stateInfo)
        { 
            var state = stateInfo.WorldState;
            var goal = stateInfo.CurrentGoal;
            
            if (!goal.Has(UGoapPropertyManager.PropertyList.Target)) return false;
            if (state.Has(UGoapPropertyManager.PropertyList.Target)) return !stateInfo.WorldState[UGoapPropertyManager.PropertyList.Target]
                    .Equals(stateInfo.CurrentGoal[UGoapPropertyManager.PropertyList.Target]);
            return true;
        }
        
        protected override PropertyGroup<UGoapPropertyManager.PropertyList, object> GetProceduralEffects(GoapStateInfo<UGoapPropertyManager.PropertyList, object> stateInfo)
        {
            PropertyGroup<UGoapPropertyManager.PropertyList, object> proceduralEffects = new PropertyGroup<UGoapPropertyManager.PropertyList, object>();
            var goal = stateInfo.CurrentGoal;
            if (goal.Has(UGoapPropertyManager.PropertyList.Target))
            {
                _target = goal[UGoapPropertyManager.PropertyList.Target];
                proceduralEffects[UGoapPropertyManager.PropertyList.Target] = _target;
                return proceduralEffects;
            }
            return null;
        }

        protected override HashSet<UGoapPropertyManager.PropertyList> GetAffectedPropertyLists()
        {
            return new HashSet<UGoapPropertyManager.PropertyList> { UGoapPropertyManager.PropertyList.Target };
        }

        protected override void PerformedActions(UGoapAgent agent)
        {
            //GO TO target
            agent.GoToTarget((string)_target);
        }

        public override int GetCost(GoapStateInfo<UGoapPropertyManager.PropertyList, object> stateInfo)
        {
            var ws = stateInfo.WorldState;
            var goal = stateInfo.CurrentGoal;

            var target1 = ws.Has(UGoapPropertyManager.PropertyList.Target) ? (string) ws[UGoapPropertyManager.PropertyList.Target] : null;
            var target2 = goal.Has(UGoapPropertyManager.PropertyList.Target) ? (string) goal[UGoapPropertyManager.PropertyList.Target] : null;

            if (target1 == null || target2 == null) return 999;
            
            var pos1 = UGoapWMM.Get(target1).Position;
            var pos2 = UGoapWMM.Get(target2).Position;

            return Math.Max(5, (int)Vector3.Distance(pos1, pos2));
        }
    }
}