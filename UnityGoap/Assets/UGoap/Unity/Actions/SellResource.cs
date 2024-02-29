using System.Linq;
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

        protected override GoapConditions<PropertyKey, object> GetProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var proceduralConditions = new GoapConditions<PropertyKey, object>();

            var resourceCount = GetRequiredAmount(stateInfo);
            
            proceduralConditions.Set(_resource, resourceCount, ConditionType.GreaterOrEqual);
            
            return proceduralConditions;
        }

        protected override GoapEffects<PropertyKey, object> GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var proceduralEffects = new GoapEffects<PropertyKey, object>();
            
            var resourceCount = GetRequiredAmount(stateInfo);
            var money = resourceCount * _price;
            
            proceduralEffects.Set(_resource, resourceCount, EffectType.Subtract);
            proceduralEffects.Set(Money, money, EffectType.Add);
            
            return proceduralEffects;
        }

        protected override void PerformedActions(GoapState<PropertyKey, object> goapState, UGoapAgent agent)
        {
            agent.GoGenericAction(_waitSeconds);
        }

        private int GetRequiredAmount(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var moneyValue = stateInfo.Goal.TryGetOrDefault(Money, 0f).First();
            float currentMoney = stateInfo.State.TryGetOrDefault(Money, 0f);
            currentMoney += stateInfo.PredictedState.TryGetOrDefault(Money, 0f);
            float moneyRequired = Mathf.Max(moneyValue.Value - currentMoney, 0f);
            if (moneyValue.ConditionType == ConditionType.GreaterThan) moneyRequired += 1;
            return (int) Mathf.Ceil(moneyRequired / _price);
        }
    }
}