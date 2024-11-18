using System;
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
        [SerializeField] private string _name;
        [SerializeField] private Vector2 _xLimits;
        [SerializeField] private Vector2 _zLimits;
        
        protected override GoapConditions GetProceduralConditions(GoapSettings settings)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapSettings settings)
        {
            var x = Mathf.RoundToInt(Random.Range(_xLimits.x, _xLimits.y));
            var z = Mathf.RoundToInt(Random.Range(_zLimits.x, _zLimits.y));
        
            var effects = new GoapEffects();
            effects.Set(PropertyKey.DestinationX, EffectType.Set, (float)x);
            effects.Set(PropertyKey.DestinationZ, EffectType.Set, (float)z);

            return effects;
        }

        public string GetName(GoapConditions conditions, GoapEffects effects)
        {
            string actionName = _name;
            actionName += effects.TryGetOrDefault(PropertyKey.DestinationX, 0f).Value + "_";
            actionName += effects.TryGetOrDefault(PropertyKey.DestinationZ, 0f).Value;
            return actionName;
        }

        
        //TODO Maybe better to check it before. Only random available postions, for better performance.
        public override bool Validate(GoapState goapState, GoapActionInfo actionInfo, IGoapAgent agent)
        {
            if (agent is not UGoapAgent uGoapAgent) return false;
            
            if (!goapState.TryGetOrDefault(PropertyKey.IsIt, false))
            {
                    var playerPosition = UGoapWMM.Get("Player").Object.transform.position;
                    var destination = playerPosition;
                    destination.x = (float)actionInfo.Effects.TryGetOrDefault(PropertyKey.DestinationX, 0f).Value;
                    destination.z = (float)actionInfo.Effects.TryGetOrDefault(PropertyKey.DestinationZ, 0f).Value;

                    var destinationDirection = destination - uGoapAgent.transform.position;
                    if (destinationDirection.magnitude < 0.1f)
                    {
                        return false;
                    }
                    
                    var playerDirection = playerPosition - uGoapAgent.transform.position;
                    if (playerDirection.magnitude > 0.1f && Vector3.Angle(destinationDirection, playerDirection) <= 45.0f)
                    {
                        return false;
                    }
            }

            return true;
        }

        public override void Execute(ref GoapState goapState, IGoapAgent agent)
        {
            if (agent is not UGoapAgent uGoapAgent) return;
            
            uGoapAgent.GoGenericAction(Name, ref goapState);
        }
    }
}