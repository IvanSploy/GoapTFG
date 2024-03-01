using System.Linq;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Base.UGoapPropertyManager;
using static UGoap.Base.UGoapPropertyManager.PropertyKey;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "SellResource", menuName = "Goap Items/Actions/SellResource", order = 1)]
    public class SellResource : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private PropertyKey _resource;
        [SerializeField] private float _price = 1;
        [SerializeField] private int _waitSeconds = 1;

        protected override bool Validate(GoapStateInfo stateInfo)
        {
            return true;
        }

        protected override GoapConditions GetProceduralConditions(GoapStateInfo stateInfo)
        {
            var proceduralConditions = new GoapConditions();

            var resourceCount = GetRequiredAmount(stateInfo);
            
            proceduralConditions.Set(_resource, resourceCount, ConditionType.GreaterOrEqual);
            
            return proceduralConditions;
        }

        protected override GoapEffects GetProceduralEffects(GoapStateInfo stateInfo)
        {
            var proceduralEffects = new GoapEffects();
            
            var resourceCount = GetRequiredAmount(stateInfo);
            var money = resourceCount * _price;
            
            proceduralEffects.Set(_resource, resourceCount, EffectType.Subtract);
            proceduralEffects.Set(Money, money, EffectType.Add);
            
            return proceduralEffects;
        }

        protected override void PerformedActions(GoapState goapState, UGoapAgent agent)
        {
            agent.GoGenericAction(_waitSeconds);
        }

        private int GetRequiredAmount(GoapStateInfo stateInfo)
        {
            var moneyValue = stateInfo.Goal.TryGetOrDefault(Money, 0f).First();
            float currentMoney = stateInfo.State.TryGetOrDefault(Money, 0f);
            currentMoney += stateInfo.PredictedState.TryGetOrDefault(Money, 0f);
            float moneyRequired = Mathf.Max((float)moneyValue.Value - currentMoney, 0f);
            if (moneyValue.ConditionType == ConditionType.GreaterThan) moneyRequired += 1;
            return (int) Mathf.Ceil(moneyRequired / _price);
        }
    }
}