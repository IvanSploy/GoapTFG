using System;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Base.UGoapPropertyManager;
using Random = UnityEngine.Random;

namespace UGoap.Unity.Actions
{
    [Serializable]
    public class SetRandomDestinationAction : GoapAction
    {
        public string Target;
        public Vector2 XLimits;
        public Vector2 ZLimits;
        
        protected override GoapConditions GetProceduralConditions(GoapSettings settings)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapSettings settings)
        {
            var x = Mathf.RoundToInt(Random.Range(XLimits.x, XLimits.y));
            var z = Mathf.RoundToInt(Random.Range(ZLimits.x, ZLimits.y));
        
            var effects = new GoapEffects();
            effects.Set(PropertyKey.DestinationX, EffectType.Set, (float)x);
            effects.Set(PropertyKey.DestinationZ, EffectType.Set, (float)z);

            return effects;
        }
        
        //TODO Maybe better to check it before. Only random available postions, for better performance.
        public override bool Validate(ref GoapState goapState, GoapActionInfo actionInfo, IGoapAgent iAgent)
        {
            if (iAgent is not UGoapAgent goapAgent) return false;
            
            if (!goapState.TryGetOrDefault(PropertyKey.IsIt, false))
            {
                var playerPosition = UGoapWMM.Get(Target).Object.transform.position;
                var destination = playerPosition;
                destination.x = (float)actionInfo.Effects.TryGetOrDefault(PropertyKey.DestinationX, 0f).Value;
                destination.z = (float)actionInfo.Effects.TryGetOrDefault(PropertyKey.DestinationZ, 0f).Value;

                var destinationDirection = destination - goapAgent.transform.position;
                if (destinationDirection.magnitude < 0.1f)
                {
                    return false;
                }
                
                var playerDirection = playerPosition - goapAgent.transform.position;
                if (playerDirection.magnitude > 0.1f && Vector3.Angle(destinationDirection, playerDirection) <= 45.0f)
                {
                    return false;
                }
            }

            return true;
        }

        public override async Task<GoapState> Execute(GoapState state, IGoapAgent iAgent, CancellationToken token)
        {
            if (iAgent is not UGoapAgent goapAgent) return null;

            return state;
        }
    }
}