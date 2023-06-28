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
        
        protected override bool ProceduralConditions(GoapStateInfo<PropertyList, object> stateInfo)
        { 
            var state = stateInfo.WorldState;
            var goal = stateInfo.CurrentGoal;
            
            if (!goal.Has(PropertyList.Target)) return false;
            if (state.Has(PropertyList.Target)) return !stateInfo.WorldState[PropertyList.Target]
                    .Equals(stateInfo.CurrentGoal[PropertyList.Target]);
            return true;
        }
        
        protected override PropertyGroup<PropertyList, object> GetProceduralEffects(GoapStateInfo<PropertyList, object> stateInfo)
        {
            PropertyGroup<PropertyList, object> proceduralEffects = new PropertyGroup<PropertyList, object>();
            var goal = stateInfo.CurrentGoal;
            if (goal.Has(PropertyList.Target))
            {
                _target = goal[PropertyList.Target];
                proceduralEffects[PropertyList.Target] = _target;
                return proceduralEffects;
            }
            return null;
        }

        protected override HashSet<PropertyList> GetAffectedPropertyLists()
        {
            return new HashSet<PropertyList> { PropertyList.Target };
        }

        protected override void PerformedActions(UGoapAgent agent)
        {
            //GO TO target
            agent.GoToTarget((string)_target);
        }

        public override int GetCost(GoapStateInfo<PropertyList, object> stateInfo)
        {
            var ws = stateInfo.WorldState;
            var goal = stateInfo.CurrentGoal;

            var target1 = ws.Has(PropertyList.Target) ? (string) ws[PropertyList.Target] : null;
            var target2 = goal.Has(PropertyList.Target) ? (string) goal[PropertyList.Target] : null;

            if (target1 == null || target2 == null) return 999;
            
            var pos1 = UGoapWMM.Get(target1).Position;
            var pos2 = UGoapWMM.Get(target2).Position;

            return Math.Max(5, (int)Vector3.Distance(pos1, pos2));
        }
    }
}