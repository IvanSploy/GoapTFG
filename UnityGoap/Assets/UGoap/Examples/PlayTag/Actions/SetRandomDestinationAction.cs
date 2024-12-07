using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity;
using UGoap.Unity.ScriptableObjects;
using static UGoap.Base.BaseTypes;
using static UGoap.Base.UGoapPropertyManager;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "SetRandomDestination", menuName = "UGoap/Actions/PlayTag/SetRandomDestination")]
public class SetRandomDestination : ActionConfig<SetRandomDestinationAction>
{
    public Vector2 XLimits;
    public Vector2 ZLimits;
    
    protected override SetRandomDestinationAction Install(SetRandomDestinationAction action)
    {
        action.Init(XLimits, ZLimits);
        return action;
    }
}

public class SetRandomDestinationAction : GoapAction
{
    public Vector2 XLimits;
    public Vector2 ZLimits;

    public void Init(Vector2 xLimits, Vector2 zLimits)
    {
        XLimits = xLimits;
        ZLimits = zLimits;
    }
    
    protected override GoapConditions GetProceduralConditions(GoapSettings settings)
    {
        return null;
    }

    protected override GoapEffects GetProceduralEffects(GoapSettings settings)
    {
        var x = Mathf.RoundToInt(Random.Range(XLimits.x, XLimits.y));
        var z = Mathf.RoundToInt(Random.Range(ZLimits.x, ZLimits.y));
    
        var effects = new GoapEffects();
        effects.Set(PropertyKey.DestinationX, EffectType.Set, (float)x);
        effects.Set(PropertyKey.DestinationZ, EffectType.Set, (float)z);

        return effects;
    }
    
    public override bool Validate(GoapState goapState, GoapActionInfo actionInfo, IGoapAgent iAgent)
    {
        if (iAgent is not UGoapAgent goapAgent) return false;
        
        if (!goapState.TryGetOrDefault(PropertyKey.IsIt, false))
        {
            var destination = new Vector3
            {
                x = (float)actionInfo.Effects.TryGetOrDefault(PropertyKey.DestinationX, 0f).Value,
                z = (float)actionInfo.Effects.TryGetOrDefault(PropertyKey.DestinationZ, 0f).Value
            };

            var destinationDirection = destination - goapAgent.transform.position;
            if (destinationDirection.magnitude < 0.1f)
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