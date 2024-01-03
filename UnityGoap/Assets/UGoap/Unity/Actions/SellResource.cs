using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Unity.UGoapPropertyManager;
using static UGoap.Unity.UGoapPropertyManager.PropertyKey;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "SellResource", menuName = "Goap Items/Actions/SellResource", order = 1)]
    public class SellResource : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private PropertyKey _resource;
        [SerializeField] private float _price = 1;
        [SerializeField] private int _waitSeconds = 1;

        protected override bool Validate(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return true;
        }

        protected override ConditionGroup<PropertyKey, object> GetProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var proceduralConditions = new ConditionGroup<PropertyKey, object>();

            var resourceCount = GetRequiredAmount(stateInfo);
            
            proceduralConditions.Set(_resource, resourceCount, ConditionType.GreaterOrEqual);
            
            return proceduralConditions;
        }

        protected override EffectGroup<PropertyKey, object> GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var proceduralEffects = new EffectGroup<PropertyKey, object>();
            
            var resourceCount = GetRequiredAmount(stateInfo);
            var money = resourceCount * _price;
            
            proceduralEffects.Set(_resource, resourceCount, EffectType.Subtract);
            proceduralEffects.Set(Money, money, EffectType.Add);
            
            return proceduralEffects;
        }

        protected override void PerformedActions(StateGroup<PropertyKey, object> state, UGoapAgent agent)
        {
            agent.GoGenericAction(_waitSeconds);
        }

        private int GetRequiredAmount(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            ConditionValue<object> moneyValue = stateInfo.Goal.TryGetOrDefault(Money, 0f);
            GoapValue<object> currentMoney = stateInfo.State.TryGetOrDefault(Money, 0f);
            float moneyRequired = (float) moneyValue.Value - (float) currentMoney.Value;
            if (moneyValue.ConditionType == ConditionType.GreaterThan) moneyRequired += 1;
            return (int) Mathf.Ceil(moneyRequired / _price);
        }
    }
}