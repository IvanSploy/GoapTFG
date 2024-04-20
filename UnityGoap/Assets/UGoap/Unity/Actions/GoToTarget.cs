using System;
using System.Linq;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "GoToTarget", menuName = "Goap Items/Actions/GoToTarget", order = 3)]
    public class GoToTarget : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private PropertyKey _targetKey;
        [SerializeField] private int _speedFactor = 1;
        [SerializeField] private string _excludedLocation = "none";
        
        protected override bool Validate(GoapStateInfo stateInfo)
        {
            return stateInfo.Goal[_targetKey].Any(condition => !condition.Value.Equals("") && condition.ConditionType == ConditionType.Equal);
        }
        
        protected override GoapConditions GetProceduralConditions(
            GoapStateInfo stateInfo)
        {
            var condition = new GoapConditions();
            condition.Set(_targetKey, _excludedLocation, ConditionType.NotEqual);
            return condition;
        }
        
        protected override GoapEffects GetProceduralEffects(
            GoapStateInfo stateInfo)
        {
            GoapEffects proceduralEffects = new GoapEffects();
            string target = "";
            var targetList = stateInfo.Goal.TryGetOrDefault(_targetKey, "");
            if (targetList != null)
            {
                var condition = targetList.FirstOrDefault(condition => condition.ConditionType == ConditionType.Equal);
                if(condition != null)
                    target = (string)condition.Value;
            }
            proceduralEffects[_targetKey] = new EffectValue(target, EffectType.Set);
            return proceduralEffects;
        }

        protected override bool PerformedActions(GoapState goapState, UGoapAgent agent)
        {
            agent.GoToTarget(goapState.TryGetOrDefault(_targetKey, ""), _speedFactor);
            return true;
        }

        public override int GetCost(GoapState state, GoapGoal goal)
        {
            if (!state.HasKey(_targetKey) || !goal.Has(_targetKey)) return 50 / _speedFactor;
            
            var target1 = (string) state[_targetKey];
            var target2 = (string) goal[_targetKey].First((condition) => condition.ConditionType == ConditionType.Equal).Value;

            var pos1 = UGoapWMM.Get(target1).Position;
            var pos2 = UGoapWMM.Get(target2).Position;
            
            var cost = Math.Max(3, (int)(Vector3.Distance(pos1, pos2) / _speedFactor));
            return cost;
        }
    }
}