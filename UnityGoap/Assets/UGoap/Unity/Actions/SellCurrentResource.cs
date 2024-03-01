using System;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Base.UGoapPropertyManager;
using static UGoap.Base.UGoapPropertyManager.PropertyKey;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "SellCurrentResource", menuName = "Goap Items/Actions/SellCurrentResource", order = 1)]
    public class SellCurrentResource : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private PropertyKey _resource;
        [SerializeField] private float _price = 1;
        [SerializeField] private int _waitSeconds = 1;

        protected override bool Validate(GoapStateInfo stateInfo)
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

        protected override GoapConditions GetProceduralConditions(GoapStateInfo stateInfo)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapStateInfo stateInfo)
        {
            var proceduralEffects = new GoapEffects();

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

        protected override void PerformedActions(GoapState goapState, UGoapAgent agent)
        {
            agent.GoGenericAction(_waitSeconds);
        }
    }
}