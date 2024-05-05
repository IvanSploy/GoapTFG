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
            bool valid = true;
            if (!goapState.TryGetOrDefault(PropertyKey.IsIt, false))
            {
                if(agent is UGoapAgent uAgent)
                {
                    var playerPosition = UGoapWMM.Get("Player").Object.transform.position;
                    var destination = playerPosition;
                    destination.x = (float)_effects.TryGetOrDefault(PropertyKey.DestinationX, 0f).Value;
                    destination.z = (float)_effects.TryGetOrDefault(PropertyKey.DestinationZ, 0f).Value;

                    var direction = destination - uAgent.transform.position;
                    var playerDirection = playerPosition - uAgent.transform.position;

                    if (Vector3.Dot(direction, playerDirection) < 0.1f)
                    {
                        valid = false;
                    }
                }
            }

            return valid;
        }

        public override void Execute(GoapState goapState, IGoapAgent agent)
        {
            
        }
    }
}