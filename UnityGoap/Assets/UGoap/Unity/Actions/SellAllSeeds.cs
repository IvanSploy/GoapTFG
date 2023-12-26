using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Unity.UGoapPropertyManager;
using static UGoap.Unity.UGoapPropertyManager.PropertyKey;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "SellAllSeeds", menuName = "Goap Items/Actions/SellAllSeeds", order = 1)]
    public class SellAllSeeds : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private int _waitSeconds = 1;
        [SerializeField] private int _price = 1;

        protected override bool ProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return true;
        }

        protected override EffectGroup<PropertyKey, object> GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var proceduralEffects = new EffectGroup<PropertyKey, object>();
            var seeds = 100;
            if(stateInfo.State.HasKey(Seeds)) seeds = (int)stateInfo.State[Seeds];
            proceduralEffects.Set(Seeds, 0, EffectType.Set);
            proceduralEffects.Set(Money, seeds * GetCost() * 0.25f, EffectType.Add);
            
            return proceduralEffects;
        }

        protected override void PerformedActions(EffectGroup<PropertyKey, object> proceduralEffects, UGoapAgent agent)
        {
            agent.GoGenericAction(_waitSeconds);
        }
    }
}