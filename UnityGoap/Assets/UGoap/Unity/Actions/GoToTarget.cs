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
        
        protected override GoapConditions GetProceduralConditions(GoapConditions goal)
        {
            var condition = new GoapConditions();
            condition.Set(_targetKey, ConditionType.NotEqual, _excludedLocation);
            return condition;
        }
        
        protected override GoapEffects GetProceduralEffects(GoapConditions goal)
        {
            GoapEffects proceduralEffects = new GoapEffects();
            string target = _excludedLocation;
            var targetList = goal.TryGetOrDefault(_targetKey, "");
            if (targetList != null)
            {
                var condition = targetList.FirstOrDefault(condition => condition.ConditionType == ConditionType.Equal);
                if(condition != null) target = (string)condition.Value;
            }
            proceduralEffects[_targetKey] = new EffectValue(target, EffectType.Set);
            return proceduralEffects;
        }
        
        public override bool ProceduralValidate(GoapState state, UGoapAgent agent)
        {
            return true;
        }

        public override void ProceduralExecute(GoapState goapState, UGoapAgent agent)
        {
            agent.GoTo(goapState.TryGetOrDefault(_targetKey, ""), _speedFactor);
        }

        public override int GetCost(GoapConditions goal)
        {
            if (!goal.Has(_targetKey)) return 50 / _speedFactor;
            
            var target = (string) goal[_targetKey].First((condition) => condition.ConditionType == ConditionType.Equal).Value;

            var pos = UGoapWMM.Get(target).Position;
            
            var cost = Math.Max(3, (int)(Vector3.Distance(Vector3.zero, pos) / _speedFactor));
            return cost;
        }
    }
}