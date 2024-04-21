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
        
        protected override GoapConditions GetProceduralConditions(UGoapGoal goal)
        {
            var condition = new GoapConditions();
            condition.Set(_targetKey, _excludedLocation, ConditionType.NotEqual);
            return condition;
        }
        
        protected override GoapEffects GetProceduralEffects(UGoapGoal goal)
        {
            GoapEffects proceduralEffects = new GoapEffects();
            string target = "";
            var targetList = goal.TryGetOrDefault(_targetKey, "");
            if (targetList != null)
            {
                var condition = targetList.FirstOrDefault(condition => condition.ConditionType == ConditionType.Equal);
                if(condition != null)
                    target = (string)condition.Value;
            }
            proceduralEffects[_targetKey] = new EffectValue(target, EffectType.Set);
            return proceduralEffects;
        }
        
        protected override bool Validate(GoapState state)
        {
            return true;
        }

        protected override bool PerformedActions(GoapState goapState, UGoapAgent agent)
        {
            agent.GoToTarget(goapState.TryGetOrDefault(_targetKey, ""), _speedFactor);
            return true;
        }

        public override int GetCost(UGoapGoal goal)
        {
            if (!goal.Has(_targetKey)) return 50 / _speedFactor;
            
            var target = (string) goal[_targetKey].First((condition) => condition.ConditionType == ConditionType.Equal).Value;

            var pos = UGoapWMM.Get(target).Position;
            
            var cost = Math.Max(3, (int)(Vector3.Distance(Vector3.zero, pos) / _speedFactor));
            return cost;
        }
    }
}