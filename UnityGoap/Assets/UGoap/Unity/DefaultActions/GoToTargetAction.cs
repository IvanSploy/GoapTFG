using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.Actions
{
    public class GoToTargetAction : GoapAction
    {
        public PropertyKey TargetKey;
        public int SpeedFactor;
        public string ExcludedLocation;
        
        protected override GoapConditions GetProceduralConditions(GoapSettings settings)
        {
            var condition = new GoapConditions();
            condition.Set(TargetKey, ConditionType.NotEqual, ExcludedLocation);
            return condition;
        }
        
        protected override GoapEffects GetProceduralEffects(GoapSettings settings)
        {
            GoapEffects proceduralEffects = new GoapEffects();
            string target = ExcludedLocation;
            var targetList = settings.Goal.TryGetOrDefault(TargetKey, "");
            if (targetList != null)
            {
                var condition = targetList.FirstOrDefault(condition => condition.ConditionType == ConditionType.Equal);
                if(condition != null) target = (string)condition.Value;
            }
            proceduralEffects[TargetKey] = new EffectValue(target, EffectType.Set);
            return proceduralEffects;
        }
        
        public override int GetCost(GoapConditions goal)
        {
            if (!goal.Has(TargetKey)) return 50 / SpeedFactor;
            
            var target = (string) goal[TargetKey].First(condition => condition.ConditionType == ConditionType.Equal).Value;

            var pos = UGoapWMM.Get(target).Position;
            
            var cost = Math.Max(3, (int)(Vector3.Distance(Vector3.zero, pos) / SpeedFactor));
            return cost;
        }
        
        public override bool Validate(ref GoapState state, GoapActionInfo actionInfo, IGoapAgent iAgent)
        {
            return true;
        }

        public override async Task<GoapState> Execute(GoapState state, IGoapAgent iAgent, CancellationToken token)
        {
            if (iAgent is not UGoapAgent goapAgent) return null;
            
            var x = state.TryGetOrDefault(PropertyKey.DestinationX, 0f);
            var z = state.TryGetOrDefault(PropertyKey.DestinationZ, 0f);
            var target = new Vector3(x, 0, z);
            
            var t = goapAgent.transform;
            
            bool reached = false;

            var speed = goapAgent.Speed * SpeedFactor;
            
            while (!reached)
            {
                if (token.IsCancellationRequested)
                {
                    state.Set(PropertyKey.MoveState, "Set");
                    return state;
                }
                var p = t.position;
                target.y = p.y;
                t.position = Vector3.MoveTowards(p, target, speed * Time.deltaTime);
                t.rotation = Quaternion.LookRotation(target - p, Vector3.up);
                target.y = p.y;
                if (Vector3.Distance(t.position, target) < float.Epsilon)
                {
                    reached = true;
                }
                await Task.Yield();
            }

            return state;
        }
    }
}