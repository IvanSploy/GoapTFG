using System;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Unity.UGoapPropertyManager;
using static UGoap.Unity.UGoapPropertyManager.PropertyKey;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "WalkToTarget", menuName = "Goap Items/Actions/WalkToTarget", order = 3)]
    public class WalkToTarget : UGoapAction
    {
        protected override bool ProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        { 
            return true;
        }
        
        protected override EffectGroup<PropertyKey, object>
            GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            EffectGroup<PropertyKey, object> proceduralEffects = new EffectGroup<PropertyKey, object>();
            
            proceduralEffects[Target] = new EffectValue<object>(stateInfo.Goal[Target], EffectType.Set);
            return proceduralEffects;
        }

        protected override void PerformedActions(EffectGroup<PropertyKey, object> proceduralEffects, UGoapAgent agent)
        {
            //GO TO target
            if (proceduralEffects.HasKey(Target))
            {
                agent.GoToTargetWalking((string)proceduralEffects[Target]);
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

            return Math.Max(6, (int)(Vector3.Distance(pos1, pos2)));
        }
    }
}