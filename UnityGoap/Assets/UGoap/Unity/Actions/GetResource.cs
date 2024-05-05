using System;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "GetResource", menuName = "Goap Items/Actions/GetResource", order = 0)]
    public class GetResource : UGoapAction
    {
        [Header("Custom Data")]
        [SerializeField] private PropertyKey _resource;
        [SerializeField] private float _count = 1;
        [SerializeField] private int _waitSeconds = 1;

        //Conditions that could be resolved by the planner.
        protected override GoapConditions GetProceduralConditions(GoapConditions goal)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapConditions goal)
        {
            var proceduralEffects = new GoapEffects();
            var fact = UGoapWMM.Get(_resource);
            
            switch (GetPropertyType(_resource))
            {
                case PropertyType.Integer:
                    var ivalue = fact.Object.CurrentGoapState.TryGetOrDefault(_resource, 0);
                    var icount = (int)Math.Min(_count, ivalue);
                    proceduralEffects.Set(_resource, EffectType.Add, icount);
                    break;
                case PropertyType.Float:
                    var fvalue = fact.Object.CurrentGoapState.TryGetOrDefault(_resource, 0f);
                    var fcount = Math.Min(_count, fvalue);
                    proceduralEffects.Set(_resource, EffectType.Add, fcount);
                    break;
                default:
                    throw new 
                        ArgumentOutOfRangeException(_resource.ToString(), "Resource has no valid resource type.");
            }
            
            
            return proceduralEffects;
        }
        
        //Conditions that couldnt be resolved by the planner.
        public override bool ProceduralValidate(GoapState state, UGoapAgent agent)
        {
            var fact = UGoapWMM.Get(_resource);
            if (fact == null) return false;
            bool valid = true;
            switch (GetPropertyType(_resource))
            {
                case PropertyType.Integer:
                    var ivalue = fact.Object.CurrentGoapState.TryGetOrDefault(_resource, 0);
                    if (ivalue <= 0) valid = false;
                    break;
                case PropertyType.Float:
                    var fvalue = fact.Object.CurrentGoapState.TryGetOrDefault(_resource, 0f);
                    if (fvalue <= 0) valid = false;
                    break;
                default:
                    throw new 
                        ArgumentOutOfRangeException(_resource.ToString(), "Resource has no valid resource type.");
            }
            return valid;
        }
        
        public override void ProceduralExecute(GoapState goapState, UGoapAgent agent)
        {
            agent.GoGenericAction(Name, goapState, _waitSeconds);

            var fact = UGoapWMM.Get(_resource);
            
            switch (GetPropertyType(_resource))
            {
                case PropertyType.Integer:
                    var ivalue = fact.Object.CurrentGoapState.TryGetOrDefault(_resource, 0);
                    var icount = (int)Math.Min(_count, ivalue);
                    fact.Object.CurrentGoapState.Set(_resource, ivalue - icount);
                    break;
                case PropertyType.Float:
                    var fvalue = fact.Object.CurrentGoapState.TryGetOrDefault(_resource, 0f);
                    var fcount = (int)Math.Min(_count, fvalue);
                    fact.Object.CurrentGoapState.Set(_resource, fvalue - fcount);
                    break;
                default:
                    throw new 
                        ArgumentOutOfRangeException(_resource.ToString(), "Resource has no valid resource type.");
            }
        }
    }
}