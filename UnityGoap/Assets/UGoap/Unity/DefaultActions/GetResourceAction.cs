using System;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.Actions
{
    public class GetResourceAction : GoapAction
    {
        public PropertyKey Resource;
        public float Count = 1;
        public int WaitSeconds = 1;

        //Conditions that could be resolved by the planner.
        protected override GoapConditions GetProceduralConditions(GoapSettings settings)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapSettings settings)
        {
            var proceduralEffects = new GoapEffects();
            var fact = UGoapWMM.Get(Resource);
            
            switch (GetPropertyType(Resource))
            {
                case PropertyType.Integer:
                    var ivalue = fact.Object.CurrentState.TryGetOrDefault(Resource, 0);
                    var icount = (int)Math.Min(Count, ivalue);
                    proceduralEffects.Set(Resource, EffectType.Add, icount);
                    break;
                case PropertyType.Float:
                    var fvalue = fact.Object.CurrentState.TryGetOrDefault(Resource, 0f);
                    var fcount = Math.Min(Count, fvalue);
                    proceduralEffects.Set(Resource, EffectType.Add, fcount);
                    break;
                default:
                    throw new 
                        ArgumentOutOfRangeException(Resource.ToString(), "Resource has no valid resource type.");
            }
            
            
            return proceduralEffects;
        }
        
        //Conditions that couldnt be resolved by the planner.
        public override bool Validate(ref GoapState state, GoapActionInfo actionInfo, IGoapAgent iAgent)
        {
            var fact = UGoapWMM.Get(Resource);
            if (fact == null) return false;
            bool valid = true;
            switch (GetPropertyType(Resource))
            {
                case PropertyType.Integer:
                    var ivalue = fact.Object.CurrentState.TryGetOrDefault(Resource, 0);
                    if (ivalue <= 0) valid = false;
                    break;
                case PropertyType.Float:
                    var fvalue = fact.Object.CurrentState.TryGetOrDefault(Resource, 0f);
                    if (fvalue <= 0) valid = false;
                    break;
                default:
                    throw new 
                        ArgumentOutOfRangeException(Resource.ToString(), "Resource has no valid resource type.");
            }
            return valid;
        }
        
        public override async Task<GoapState> Execute(GoapState goapState, IGoapAgent iAgent, CancellationToken token)
        {
            var fact = UGoapWMM.Get(Resource);
            
            switch (GetPropertyType(Resource))
            {
                case PropertyType.Integer:
                    var ivalue = fact.Object.CurrentState.TryGetOrDefault(Resource, 0);
                    var icount = (int)Math.Min(Count, ivalue);
                    fact.Object.CurrentState.Set(Resource, ivalue - icount);
                    break;
                case PropertyType.Float:
                    var fvalue = fact.Object.CurrentState.TryGetOrDefault(Resource, 0f);
                    var fcount = (int)Math.Min(Count, fvalue);
                    fact.Object.CurrentState.Set(Resource, fvalue - fcount);
                    break;
                default:
                    throw new 
                        ArgumentOutOfRangeException(Resource.ToString(), "Resource has no valid resource type.");
            }

            await Task.Delay(WaitSeconds * 1000, token);
            return goapState;
        }
    }
}