using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.Actions
{
    public class SetDestinationToTargetAction : GoapAction
    {
        public string Target;
        
        protected override GoapConditions GetProceduralConditions(GoapSettings settings)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapSettings settings)
        {
            return null;
        }

        //public override int GetCost(GoapConditions goal)
        //{
        //    return Random.Range(2, 50);
        //}

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
            
            UGoapEntity entityPlayer = UGoapWMM.Get(Target).Object;
            var p = entityPlayer.transform.position;
            state.Set(PropertyKey.DestinationX, p.x);
            state.Set(PropertyKey.DestinationZ, p.z);
            
            return state;
        }
    }
}