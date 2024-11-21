using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity;
using UGoap.Unity.ScriptableObjects;

[CreateAssetMenu(fileName = "SetDestinationToTarget", menuName = "UGoap/Actions/PlayTag/SetDestinationToTarget")]
public class SetDestinationToTarget : ActionConfig<SetDestinationToTargetAction>
{
    public string Target = "None";
    protected override SetDestinationToTargetAction Install(SetDestinationToTargetAction action)
    {
        action.Target = Target;
        return action;
    }
}

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

    public override bool Validate(GoapState goapState, GoapActionInfo actionInfo, IGoapAgent iAgent)
    {
        if (iAgent is not UGoapAgent goapAgent) return false;
        
        if (!goapState.TryGetOrDefault(UGoapPropertyManager.PropertyKey.IsIt, false))
        {
            var playerPosition = UGoapWMM.Get(Target).Object.transform.position;
            var destination = playerPosition;
            destination.x = (float)actionInfo.Effects.TryGetOrDefault(UGoapPropertyManager.PropertyKey.DestinationX, 0f).Value;
            destination.z = (float)actionInfo.Effects.TryGetOrDefault(UGoapPropertyManager.PropertyKey.DestinationZ, 0f).Value;

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
        state.Set(UGoapPropertyManager.PropertyKey.DestinationX, p.x);
        state.Set(UGoapPropertyManager.PropertyKey.DestinationZ, p.z);
        
        return state;
    }
}