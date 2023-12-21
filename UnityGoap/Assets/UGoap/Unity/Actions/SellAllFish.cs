using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;
using static GoapTFG.UGoap.UGoapPropertyManager.PropertyKey;

namespace GoapTFG.UGoap.Actions
{
    [CreateAssetMenu(fileName = "SellAllFish", menuName = "Goap Items/Actions/SellAllFish", order = 1)]
    public class SellAllFish : UGoapAction
    {
        protected override bool ProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return true;
        }

        protected override PropertyGroup<PropertyKey, object> GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var proceduralEffects = new PropertyGroup<PropertyKey, object>();
            var fish = 100;
            if(stateInfo.State.HasKey(Fish)) fish = (int) stateInfo.State[Fish];
            proceduralEffects.Set(Fish, 0, BaseTypes.EffectType.Set);
            proceduralEffects.Set(Money, fish * GetCost() * 0.25f, BaseTypes.EffectType.Add);
            
            return proceduralEffects;
        }

        protected override void PerformedActions(PropertyGroup<PropertyKey, object> proceduralEffects, UGoapAgent agent)
        {
            agent.GoGenericAction(GetCost());
        }
    }
}