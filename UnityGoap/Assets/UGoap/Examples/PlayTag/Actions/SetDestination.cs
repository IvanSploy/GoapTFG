using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity;
using UGoap.Unity.ScriptableObjects;
using UGoap.Learning;
using System.Globalization;

[CreateAssetMenu(fileName = "SetDestination", menuName = "UGoap/Actions/PlayTag/SetDestination")]
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

public class SetDestinationAction : GoapAction
{
    private float _destinationX;
    private float _destinationZ;

    public void Init(float x, float z)
    {
        _destinationX = x;
        _destinationZ = z;
    }
    
    protected override GoapConditions GetProceduralConditions(GoapSettings settings)
    {
        return null;
    }

    protected override GoapEffects GetProceduralEffects(GoapSettings settings)
    {
        GoapEffects goapEffects = new GoapEffects();

        goapEffects.Set(UGoapPropertyManager.PropertyKey.DestinationX, BaseTypes.EffectType.Set, _destinationX);
        goapEffects.Set(UGoapPropertyManager.PropertyKey.DestinationZ, BaseTypes.EffectType.Set, _destinationZ);
        return goapEffects;
    }

    public override bool Validate(GoapState goapState, GoapActionInfo actionInfo, IGoapAgent iAgent)
    {
        if (iAgent is not UGoapAgent goapAgent) return false;
        
        if (!goapState.TryGetOrDefault(UGoapPropertyManager.PropertyKey.IsIt, false))
        {
            var playerPosition = UGoapWMM.Get("Player").Object.transform.position;
            var destination = playerPosition;
            destination.x = _destinationX;
            destination.z = _destinationZ;

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
        
        return state;
    }
}