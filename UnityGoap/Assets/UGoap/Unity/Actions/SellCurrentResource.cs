using System;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Unity.UGoapPropertyManager;
using static UGoap.Unity.UGoapPropertyManager.PropertyKey;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "SellCurrentResource", menuName = "Goap Items/Actions/SellCurrentResource", order = 1)]
    public class SellCurrentResource : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private PropertyKey _resource;
        [SerializeField] private float _price = 1;
        [SerializeField] private int _waitSeconds = 1;

        protected override bool Validate(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            bool valid = true;
            switch (GetPropertyType(_resource))
            {
                case PropertyType.Integer:
                    var ivalue = stateInfo.State.TryGetOrDefault(_resource, 0);
                    if (ivalue <= 0) valid = false;
                    break;
                case PropertyType.Float:
                    var fvalue = stateInfo.State.TryGetOrDefault(_resource, 0f);
                    if (fvalue <= 0) valid = false;
                    break;
                default:
                    throw new 
                        ArgumentOutOfRangeException(_resource.ToString(), "Resource has no valid resource type.");
            }
            return valid;
        }

        protected override GoapConditions<PropertyKey, object> GetProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return null;
        }

        protected override GoapEffects<PropertyKey, object> GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var proceduralEffects = new GoapEffects<PropertyKey, object>();

            float resourceCount;
            switch (GetPropertyType(_resource))
            {
                case PropertyType.Integer:
                    var icount = stateInfo.State.TryGetOrDefault(_resource, 0);
                    proceduralEffects.Set(_resource, icount, EffectType.Subtract);
                    resourceCount = icount;
                    break;
                case PropertyType.Float:
                    var fcount = stateInfo.State.TryGetOrDefault(_resource, 0f);
                    proceduralEffects.Set(_resource, fcount, EffectType.Subtract);
                    resourceCount = fcount;
                    break;
                default:
                    throw new 
                        ArgumentOutOfRangeException(_resource.ToString(), "Resource has no valid resource type.");
            }
            var money = resourceCount * _price;
            
            proceduralEffects.Set(Money, money, EffectType.Add);
            
            return proceduralEffects;
        }

        protected override void PerformedActions(GoapState<PropertyKey, object> goapState, UGoapAgent agent)
        {
            agent.GoGenericAction(_waitSeconds);
        }
    }
}