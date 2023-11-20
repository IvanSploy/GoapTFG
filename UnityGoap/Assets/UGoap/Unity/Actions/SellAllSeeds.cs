using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap.Actions
{
    [CreateAssetMenu(fileName = "SellAllSeeds", menuName = "Goap Items/Actions/SellAllSeeds", order = 1)]
    public class SellAllSeeds : UGoapAction
    {
        protected override bool ProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return true;
        }

        //TODO Not working properly
        protected override PropertyGroup<PropertyKey, object> GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var proceduralEffects = new PropertyGroup<PropertyKey, object>();
            var seeds = 100;
            if(stateInfo.WorldState.HasKey(PropertyKey.Seeds)) seeds = (int)stateInfo.WorldState[PropertyKey.Seeds];
            proceduralEffects.Set(PropertyKey.Seeds, 0, BaseTypes.EffectType.Set);
            proceduralEffects.Set(PropertyKey.Money, seeds * GetCost() * 0.25f, BaseTypes.EffectType.Add);
            
            return proceduralEffects;
        }

        protected override void PerformedActions(UGoapAgent agent)
        {
            agent.GoGenericAction(GetCost());
        }
    }
}