using System;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Unity.UGoapPropertyManager;
using static UGoap.Unity.UGoapPropertyManager.PropertyKey;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "GoToTarget", menuName = "Goap Items/Actions/GoToTarget", order = 3)]
    public class GoToTarget : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private int _speedFactor = 1;
        [SerializeField] private int _minDistance = 0;
        
        protected override bool ProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            if (!stateInfo.State.HasKey(Target)) return true;
            var initialTarget = (string) stateInfo.State[Target]; 
            var finalTarget = (string) stateInfo.Goal[Target];

            var initialPos = UGoapWMM.Get(initialTarget).Position;
            var finalPos = UGoapWMM.Get(finalTarget).Position;

            return Vector3.Distance(initialPos, finalPos) > _minDistance;
        }
        
        protected override EffectGroup<PropertyKey, object> GetProceduralEffects(
            GoapStateInfo<PropertyKey, object> stateInfo)
        {
            EffectGroup<PropertyKey, object> proceduralEffects = new EffectGroup<PropertyKey, object>();
            proceduralEffects[Target] = new EffectValue<object>(stateInfo.Goal[Target], EffectType.Set);
            return proceduralEffects;
        }

        protected override void PerformedActions(EffectGroup<PropertyKey, object> proceduralEffects, UGoapAgent agent)
        {
            agent.GoToTarget((string)proceduralEffects[Target], _speedFactor);
        }

        public override int GetCost(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var ws = stateInfo.State;
            var goal = stateInfo.Goal;

            if (!ws.HasKey(Target) || !goal.Has(Target)) return 50 / _speedFactor;
            
            var target1 = (string) ws[Target];
            var target2 = (string) goal[Target];

            var pos1 = UGoapWMM.Get(target1).Position;
            var pos2 = UGoapWMM.Get(target2).Position;

            return Math.Max(3, (int)(Vector3.Distance(pos1, pos2) / _speedFactor));
        }
    }
}