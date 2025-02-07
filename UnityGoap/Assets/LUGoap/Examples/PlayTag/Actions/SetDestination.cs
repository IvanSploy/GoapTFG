using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;

public class SetDestinationAction : Action
{
    public float DestinationX;
    public float DestinationZ;

    private Transform _transform;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _transform = agent.transform;
    }

    protected override Effects GetProceduralEffects(ActionSettings settings)
    {
        Effects effects = new Effects();

        effects.Set(PropertyManager.PropertyKey.DestinationX, BaseTypes.EffectType.Set, DestinationX);
        effects.Set(PropertyManager.PropertyKey.DestinationZ, BaseTypes.EffectType.Set, DestinationZ);
        return effects;
    }

    protected override bool OnValidate(State nextState, string[] parameters)
    {
        if (!_agent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.IsIt, false))
        {
            var playerPosition = WorkingMemoryManager.Get("Player").Object.transform.position;
            var destination = playerPosition;
            destination.x = DestinationX;
            destination.z = DestinationZ;

            var destinationDirection = destination - _transform.position;
            if (destinationDirection.magnitude < 0.1f)
            {
                return false;
            }
            
            var playerDirection = playerPosition - _transform.position;
            if (playerDirection.magnitude > 0.1f && Vector3.Angle(destinationDirection, playerDirection) <= 45.0f)
            {
                return false;
            }
        }

        return true;
    }

    protected override async Task<Effects> OnExecute(Effects effects, string[] parameters, CancellationToken token)
    {
        
        return effects;
    }
}