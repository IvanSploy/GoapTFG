﻿using System;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using static LUGoap.Base.PropertyManager;

namespace LUGoap.Unity.Action
{
    public class GetResourceAction : Base.Action
    {
        public PropertyKey Resource;
        public float Count = 1;
        public int WaitSeconds = 1;

        protected override void Init() { }

        protected override Effects GetProceduralEffects(ActionSettings settings)
        {
            var proceduralEffects = new Effects();
            var fact = WorkingMemoryManager.Get(Resource);
            
            switch (GetPropertyType(Resource))
            {
                case PropertyType.Integer:
                    var ivalue = fact.Object.CurrentState.TryGetOrDefault(Resource, 0);
                    var icount = (int)Math.Min(Count, ivalue);
                    proceduralEffects.Set(Resource, BaseTypes.EffectType.Add, icount);
                    break;
                case PropertyType.Float:
                    var fvalue = fact.Object.CurrentState.TryGetOrDefault(Resource, 0f);
                    var fcount = Math.Min(Count, fvalue);
                    proceduralEffects.Set(Resource, BaseTypes.EffectType.Add, fcount);
                    break;
                default:
                    throw new 
                        ArgumentOutOfRangeException(Resource.ToString(), "Resource has no valid resource type.");
            }
            
            
            return proceduralEffects;
        }
        
        //Conditions that couldn't be resolved by the planner.
        protected override bool OnValidate(State nextState, string[] parameters)
        {
            var fact = WorkingMemoryManager.Get(Resource);
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
        
        protected override async Task<Effects> OnExecute(Effects effects, string[] parameters, CancellationToken token)
        {
            var fact = WorkingMemoryManager.Get(Resource);
            
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
                    token.ThrowIfCancellationRequested();
                    break;
            }

            await Task.Delay(WaitSeconds * 1000, token);
            return effects;
        }
    }
}