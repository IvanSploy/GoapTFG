using UGoap.Base;
using UnityEngine;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.Actions
{
    public class SetDestinationAction : GoapAction
    {
        public SetDestinationAction(string name, GoapConditions conditions, GoapEffects effects) : base(name, conditions, effects)
        { }

        protected override GoapConditions GetProceduralConditions(GoapConditions goal)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapConditions goal)
        {
            return null;
        }

        public override int GetCost(GoapConditions goal)
        {
            return Random.Range(2, 50);
        }

        public override bool Validate(GoapState goapState, IGoapAgent agent)
        {
            if (!goapState.TryGetOrDefault(PropertyKey.IsIt, false))
            {
                if(agent is UGoapAgent uAgent)
                {
                    var playerPosition = UGoapWMM.Get("Player").Object.transform.position;
                    var destination = playerPosition;
                    destination.x = (float)_effects.TryGetOrDefault(PropertyKey.DestinationX, 0f).Value;
                    destination.z = (float)_effects.TryGetOrDefault(PropertyKey.DestinationZ, 0f).Value;

                    var destinationDirection = destination - uAgent.transform.position;
                    if (destinationDirection.magnitude < 0.1f)
                    {
                        return false;
                    }
                    
                    var playerDirection = playerPosition - uAgent.transform.position;
                    if (playerDirection.magnitude > 0.1f && Vector3.Angle(destinationDirection, playerDirection) <= 45.0f)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override void Execute(ref GoapState goapState, IGoapAgent agent)
        {
            if (agent is UGoapAgent uAgent)
            {
                uAgent.GoGenericAction(Name, ref goapState);
            }
        }
    }
}