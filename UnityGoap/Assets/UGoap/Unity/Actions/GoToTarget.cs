using System;
using System.Linq;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Base.UGoapPropertyManager.PropertyKey;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "GoToTarget", menuName = "Goap Items/Actions/GoToTarget", order = 3)]
    public class GoToTarget : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private int _speedFactor = 1;
        [SerializeField] private string _excludedLocation = "none";
        
        protected override bool Validate(GoapStateInfo stateInfo)
        {
            return stateInfo.Goal[Target].Any(condition => !condition.Value.Equals("") && condition.ConditionType == ConditionType.Equal);
        }
        
        protected override GoapConditions GetProceduralConditions(
            GoapStateInfo stateInfo)
        {
            var condition = new GoapConditions();
            condition.Set(Target, _excludedLocation, ConditionType.NotEqual);
            return condition;
        }
        
        protected override GoapEffects GetProceduralEffects(
            GoapStateInfo stateInfo)
        {
            GoapEffects proceduralEffects = new GoapEffects();
            string target = "";
            var targetList = stateInfo.Goal.TryGetOrDefault(Target, "");
            if (targetList != null)
            {
                var condition = targetList.FirstOrDefault(condition => condition.ConditionType == ConditionType.Equal);
                if(condition != null)
                    target = (string)condition.Value;
            }
            proceduralEffects[Target] = new EffectValue(target, EffectType.Set);
            return proceduralEffects;
        }

        protected override void PerformedActions(GoapState goapState, UGoapAgent agent)
        {
            agent.GoToTarget(goapState.TryGetOrDefault(Target, ""), _speedFactor);
        }

        public override int GetCost(GoapState state, GoapGoal goal)
        {
            if (!state.HasKey(Target) || !goal.Has(Target)) return 50 / _speedFactor;
            
            var target1 = (string) state[Target];
            var target2 = (string) goal[Target].First((condition) => condition.ConditionType == ConditionType.Equal).Value;

            var pos1 = UGoapWMM.Get(target1).Position;
            var pos2 = UGoapWMM.Get(target2).Position;
            
            var cost = Math.Max(3, (int)(Vector3.Distance(pos1, pos2) / _speedFactor));
            return cost;
        }
    }
}