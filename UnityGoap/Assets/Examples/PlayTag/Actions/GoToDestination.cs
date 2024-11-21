using System.Threading;
using UnityEngine;
using UGoap.Base;
using System.Threading.Tasks;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.Actions
{
    public class GoToDestinationAction : GoapAction
    {
        public int SpeedFactor = 1;
        
        protected override GoapConditions GetProceduralConditions(GoapSettings settings)
        {
            return null;
        }
        
        protected override GoapEffects GetProceduralEffects(GoapSettings settings)
        {
            return null;
        }
        
        public override bool Validate(ref GoapState state, GoapActionInfo actionInfo, IGoapAgent iAgent)
        {
            if (iAgent is not UGoapAgent agent) return false;
            
            return true;
        }

        public override async Task<GoapState> Execute(GoapState state, IGoapAgent iAgent, CancellationToken token)
        {
            if (iAgent is not UGoapAgent agent) return null;
            
            var x = state.TryGetOrDefault(PropertyKey.DestinationX, 0f);
            var z = state.TryGetOrDefault(PropertyKey.DestinationZ, 0f);
            var target = new Vector3(x, 0, z);
            
            var t = agent.transform;
            
            bool reached = false;

            var speed = agent.Speed * SpeedFactor;
            
            while (!reached)
            {
                if (token.IsCancellationRequested) return state;

                var p = t.position;
                target.y = p.y;
                t.position = Vector3.MoveTowards(p, target, speed * Time.deltaTime);
                t.rotation = Quaternion.LookRotation(target - p, Vector3.up);
                target.y = p.y;
                if (Vector3.Distance(t.position, target) < float.Epsilon)
                {
                    reached = true;
                }
                await Task.Yield();
            }

            return state;
        }
    }
}