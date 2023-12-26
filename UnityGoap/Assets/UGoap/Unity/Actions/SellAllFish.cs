using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Unity.UGoapPropertyManager;
using static UGoap.Unity.UGoapPropertyManager.PropertyKey;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "SellAllFish", menuName = "Goap Items/Actions/SellAllFish", order = 1)]
    public class SellAllFish : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private int _waitSeconds = 1;

        protected override bool ProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return true;
        }

        protected override EffectGroup<PropertyKey, object> GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var proceduralEffects = new EffectGroup<PropertyKey, object>();
            var fish = 100;
            if(stateInfo.State.HasKey(Fish)) fish = (int) stateInfo.State[Fish];
            proceduralEffects.Set(Fish, 0, EffectType.Set);
            proceduralEffects.Set(Money, fish * GetCost() * 0.25f, EffectType.Add);
            
            return proceduralEffects;
        }

        protected override void PerformedActions(EffectGroup<PropertyKey, object> proceduralEffects, UGoapAgent agent)
        {
            agent.GoGenericAction(_waitSeconds);
        }
    }
}