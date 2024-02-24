using System;
using System.Collections.Generic;
using System.Linq;
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
        [SerializeField] private string _excludedLocation = "none";
        
        protected override bool Validate(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return stateInfo.Goal[Target].Any(condition => !condition.Value.Equals("") && condition.ConditionType == ConditionType.Equal);
        }
        
        protected override ConditionGroup<PropertyKey, object> GetProceduralConditions(
            GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var condition = new ConditionGroup<PropertyKey, object>();
            condition.Set(Target, _excludedLocation, ConditionType.NotEqual);
            return condition;
        }
        
        protected override EffectGroup<PropertyKey, object> GetProceduralEffects(
            GoapStateInfo<PropertyKey, object> stateInfo)
        {
            EffectGroup<PropertyKey, object> proceduralEffects = new EffectGroup<PropertyKey, object>();
            string target = "";
            var targetList = stateInfo.Goal.TryGetOrDefault<string>(Target, null);
            if (targetList != null)
            {
                var condition = targetList.
                    FirstOrDefault(condition => condition.ConditionType == ConditionType.Equal);
                if(condition != null)
                    target = condition.Value;
            }
            else
            {
                Debug.LogError("This should not happen.");
            }
            proceduralEffects[Target] = new EffectValue<object>(target, EffectType.Set);
            return proceduralEffects;
        }

        protected override void PerformedActions(StateGroup<PropertyKey, object> state, UGoapAgent agent)
        {
            agent.GoToTarget(state.TryGetOrDefault(Target, ""), _speedFactor);
        }

        public override int GetCost(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var ws = stateInfo.State;
            var goal = stateInfo.Goal;

            if (!ws.HasKey(Target) || !goal.Has(Target)) return 50 / _speedFactor;
            
            var target1 = (string) ws[Target];
            var target2 = (string) stateInfo.Goal[Target].First((condition) => condition.ConditionType == ConditionType.Equal).Value;

            var pos1 = UGoapWMM.Get(target1).Position;
            var pos2 = UGoapWMM.Get(target2).Position;
            
            var cost = Math.Max(3, (int)(Vector3.Distance(pos1, pos2) / _speedFactor));
            return cost;
        }
    }
}