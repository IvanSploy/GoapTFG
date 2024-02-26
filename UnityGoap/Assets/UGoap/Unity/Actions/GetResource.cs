using System;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Unity.UGoapPropertyManager;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "GetResource", menuName = "Goap Items/Actions/GetResource", order = 0)]
    public class GetResource : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private PropertyKey _resource;
        [SerializeField] private float _count = 1;
        [SerializeField] private int _waitSeconds = 1;

        //Conditions that couldnt be resolved by the planner.
        protected override bool Validate(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var fact = UGoapWMM.Get(_resource);
            if (fact == null) return false;
            bool valid = true;
            switch (GetPropertyType(_resource))
            {
                case PropertyType.Integer:
                    var ivalue = fact.Object.CurrentState.TryGetOrDefault(_resource, 0);
                    if (ivalue <= 0) valid = false;
                    break;
                case PropertyType.Float:
                    var fvalue = fact.Object.CurrentState.TryGetOrDefault(_resource, 0f);
                    if (fvalue <= 0) valid = false;
                    break;
                default:
                    throw new 
                        ArgumentOutOfRangeException(_resource.ToString(), "Resource has no valid resource type.");
            }
            return valid;
        }

        //Conditions that could be resolved by the planner.
        protected override ConditionGroup<PropertyKey, object> GetProceduralConditions(
            GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return null;
        }

        protected override EffectGroup<PropertyKey, object> GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var proceduralEffects = new EffectGroup<PropertyKey, object>();
            var fact = UGoapWMM.Get(_resource);
            
            switch (GetPropertyType(_resource))
            {
                case PropertyType.Integer:
                    var ivalue = fact.Object.CurrentState.TryGetOrDefault(_resource, 0);
                    var icount = (int)Math.Min(_count, ivalue);
                    proceduralEffects.Set(_resource, icount, EffectType.Add);
                    break;
                case PropertyType.Float:
                    var fvalue = fact.Object.CurrentState.TryGetOrDefault(_resource, 0f);
                    var fcount = Math.Min(_count, fvalue);
                    proceduralEffects.Set(_resource, fcount, EffectType.Add);
                    break;
                default:
                    throw new 
                        ArgumentOutOfRangeException(_resource.ToString(), "Resource has no valid resource type.");
            }
            
            
            return proceduralEffects;
        }
        
        protected override void PerformedActions(StateGroup<PropertyKey, object> state, UGoapAgent agent)
        {
            agent.GoGenericAction(_waitSeconds);
            
            var fact = UGoapWMM.Get(_resource);
            
            switch (GetPropertyType(_resource))
            {
                case PropertyType.Integer:
                    var ivalue = fact.Object.CurrentState.TryGetOrDefault(_resource, 0);
                    var icount = (int)Math.Min(_count, ivalue);
                    fact.Object.CurrentState.Set(_resource, ivalue - icount);
                    break;
                case PropertyType.Float:
                    var fvalue = fact.Object.CurrentState.TryGetOrDefault(_resource, 0f);
                    var fcount = (int)Math.Min(_count, fvalue);
                    fact.Object.CurrentState.Set(_resource, fvalue - fcount);
                    break;
                default:
                    throw new 
                        ArgumentOutOfRangeException(_resource.ToString(), "Resource has no valid resource type.");
            }
        }
    }
}