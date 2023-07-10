using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap.Actions
{
    [CreateAssetMenu(fileName = "SellAllWood", menuName = "Goap Items/Actions/SellAllWood", order = 1)]
    public class SellAllWoodAction : UGoapAction
    {
        protected override bool ProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return true;
        }

        protected override PropertyGroup<PropertyKey, object> GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var effects = new PropertyGroup<PropertyKey, object>();
            var wood = (int)stateInfo.WorldState[PropertyKey.WoodCount];
            effects.Set(PropertyKey.WoodCount, 0, BaseTypes.EffectType.Set);
            effects.Set(PropertyKey.GoldCount, wood * 0.25f, BaseTypes.EffectType.Add);
            
            return effects;
        }

        protected override void PerformedActions(UGoapAgent agent) { }
    }
}