using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;
using LUGoap.Unity.ScriptableObjects;

[CreateAssetMenu(fileName = "SetDestinationToTarget", menuName = "LUGoap/Actions/PlayTag/SetDestinationToTarget")]
public class SetDestinationToTarget : ActionConfig<SetDestinationToTargetAction>
{
    public string Target = "None";
    protected override SetDestinationToTargetAction Install(SetDestinationToTargetAction action)
    {
        action.Target = Target;
        return action;
    }
}

public class SetDestinationToTargetAction : Action
{
    public string Target;

    protected override Conditions GetProceduralConditions(ActionSettings settings)
    {
        return null;
    }

    protected override Effects GetProceduralEffects(ActionSettings settings)
    {
        return null;
    }

    //public override int GetCost(GoapConditions goal)
    //{
    //    return Random.Range(2, 50);
    //}

    protected override bool OnValidate(State nextState, IAgent iAgent, string[] parameters)
    {
        if (iAgent is not Agent agent) return false;
        
        if (!iAgent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.IsIt, false))
        {
            var targetPosition = WorkingMemoryManager.Get(Target).Object.transform.position;
            var destination = targetPosition;
            destination.x = nextState.TryGetOrDefault(PropertyManager.PropertyKey.DestinationX, 0f);
            destination.z = nextState.TryGetOrDefault(PropertyManager.PropertyKey.DestinationZ, 0f);

            var destinationDirection = destination - agent.transform.position;
            if (destinationDirection.magnitude < 0.1f)
            {
                return false;
            }

            var targetDirection = targetPosition - agent.transform.position;
            if (targetDirection.magnitude > 0.1f && Vector3.Angle(destinationDirection, targetDirection) <= 45.0f)
            {
                return false;
            }
        }

        return true;
    }

    protected override async Task<Effects> OnExecute(Effects effects, IAgent iAgent, string[] parameters, CancellationToken token)
    {
        if (iAgent is not Agent agent) return null;
        
        UEntity entityPlayer = WorkingMemoryManager.Get(Target).Object;
        var p = entityPlayer.transform.position;
        effects.Set(PropertyManager.PropertyKey.DestinationX, BaseTypes.EffectType.Set, p.x);
        effects.Set(PropertyManager.PropertyKey.DestinationZ, BaseTypes.EffectType.Set, p.z);
        
        return effects;
    }
}