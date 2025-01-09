using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;
using LUGoap.Unity.ScriptableObjects;
using LUGoap.Learning;
using System.Globalization;

[CreateAssetMenu(fileName = "SetDestination", menuName = "LUGoap/Actions/PlayTag/SetDestination")]
public class SetDestination : ActionConfig<SetDestinationAction>
{
    public float DestinationX;
    public float DestinationZ;
    
    protected override SetDestinationAction Install(SetDestinationAction action)
    {
        action.Init(DestinationX, DestinationZ);
        return action;
    }
}

public class SetDestinationAction : Action
{
    private float _destinationX;
    private float _destinationZ;

    public void Init(float x, float z)
    {
        _destinationX = x;
        _destinationZ = z;
    }
    
    protected override Conditions GetProceduralConditions(ActionSettings settings)
    {
        return null;
    }

    protected override Effects GetProceduralEffects(ActionSettings settings)
    {
        Effects effects = new Effects();

        effects.Set(PropertyManager.PropertyKey.DestinationX, BaseTypes.EffectType.Set, _destinationX);
        effects.Set(PropertyManager.PropertyKey.DestinationZ, BaseTypes.EffectType.Set, _destinationZ);
        return effects;
    }

    protected override bool OnValidate(State nextState, IAgent iAgent, string[] parameters)
    {
        if (iAgent is not GoapAgent agent) return false;
        
        if (!iAgent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.IsIt, false))
        {
            var playerPosition = WorkingMemoryManager.Get("Player").Object.transform.position;
            var destination = playerPosition;
            destination.x = _destinationX;
            destination.z = _destinationZ;

            var destinationDirection = destination - agent.transform.position;
            if (destinationDirection.magnitude < 0.1f)
            {
                return false;
            }
            
            var playerDirection = playerPosition - agent.transform.position;
            if (playerDirection.magnitude > 0.1f && Vector3.Angle(destinationDirection, playerDirection) <= 45.0f)
            {
                return false;
            }
        }

        return true;
    }

    protected override async Task<Effects> OnExecute(Effects effects, IAgent iAgent, string[] parameters, CancellationToken token)
    {
        if (iAgent is not GoapAgent agent) return null;
        
        return effects;
    }
}