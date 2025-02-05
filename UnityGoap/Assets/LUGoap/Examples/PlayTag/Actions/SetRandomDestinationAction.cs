using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;
using static LUGoap.Base.BaseTypes;
using static LUGoap.Base.PropertyManager;
using Random = UnityEngine.Random;

public class SetRandomDestinationAction : Action
{
    public Vector2 XLimits;
    public Vector2 ZLimits;

    public void Init(Vector2 xLimits, Vector2 zLimits)
    {
        XLimits = xLimits;
        ZLimits = zLimits;
    }

    protected override Conditions GetProceduralConditions(ActionSettings settings)
    {
        return null;
    }

    protected override Effects GetProceduralEffects(ActionSettings settings)
    {
        var x = Mathf.RoundToInt(Random.Range(XLimits.x, XLimits.y));
        var z = Mathf.RoundToInt(Random.Range(ZLimits.x, ZLimits.y));
    
        var effects = new Effects();
        effects.Set(PropertyKey.DestinationX, EffectType.Set, (float)x);
        effects.Set(PropertyKey.DestinationZ, EffectType.Set, (float)z);

        return effects;
    }
    
    protected override bool OnValidate(State nextState, IAgent iAgent, string[] parameters)
    {
        if (iAgent is not GoapAgent agent) return false;
        
        if (!iAgent.CurrentState.TryGetOrDefault(PropertyKey.IsIt, false))
        {
            var destination = new Vector3
            {
                x = nextState.TryGetOrDefault(PropertyKey.DestinationX, 0f),
                z = nextState.TryGetOrDefault(PropertyKey.DestinationZ, 0f)
            };

            var destinationDirection = destination - agent.transform.position;
            if (destinationDirection.magnitude < 0.1f)
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