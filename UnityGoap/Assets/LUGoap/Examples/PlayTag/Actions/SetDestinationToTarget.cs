using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;

public class SetDestinationToTargetAction : Action
{
    public string Target;

    private Transform _transform;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _transform = agent.transform;
    }

    protected override bool OnValidate(State nextState, string[] parameters)
    {
        if (!_agent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.IsIt, false))
        {
            var targetPosition = WorkingMemoryManager.Get(Target).Object.transform.position;
            var destination = targetPosition;
            destination.x = nextState.TryGetOrDefault(PropertyManager.PropertyKey.DestinationX, 0f);
            destination.z = nextState.TryGetOrDefault(PropertyManager.PropertyKey.DestinationZ, 0f);

            var destinationDirection = destination - _transform.position;
            if (destinationDirection.magnitude < 0.1f)
            {
                return false;
            }

            var targetDirection = targetPosition - _transform.position;
            if (targetDirection.magnitude > 0.1f && Vector3.Angle(destinationDirection, targetDirection) <= 45.0f)
            {
                return false;
            }
        }

        return true;
    }

    protected override async Task<Effects> OnExecute(Effects effects, string[] parameters, CancellationToken token)
    {
        GoapEntity entityPlayer = WorkingMemoryManager.Get(Target).Object;
        var p = entityPlayer.transform.position;
        effects.Set(PropertyManager.PropertyKey.DestinationX, BaseTypes.EffectType.Set, p.x);
        effects.Set(PropertyManager.PropertyKey.DestinationZ, BaseTypes.EffectType.Set, p.z);
        
        return effects;
    }
}