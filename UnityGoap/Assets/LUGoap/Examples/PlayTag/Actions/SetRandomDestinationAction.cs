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

    private Transform _transform;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _transform = agent.transform;
    }

    protected override EffectGroup GetProceduralEffects(ActionSettings settings)
    {
        var x = Mathf.RoundToInt(Random.Range(XLimits.x, XLimits.y));
        var z = Mathf.RoundToInt(Random.Range(ZLimits.x, ZLimits.y));
    
        var effects = new EffectGroup();
        effects.Set(PropertyKey.DestinationX, EffectType.Set, (float)x);
        effects.Set(PropertyKey.DestinationZ, EffectType.Set, (float)z);

        return effects;
    }
    
    protected override bool OnValidate(State nextState, string[] parameters)
    {
        if (!_agent.CurrentState.TryGetOrDefault(PropertyKey.IsIt, false))
        {
            var destination = new Vector3
            {
                x = nextState.TryGetOrDefault(PropertyKey.DestinationX, 0f),
                z = nextState.TryGetOrDefault(PropertyKey.DestinationZ, 0f)
            };

            var destinationDirection = destination - _transform.position;
            if (destinationDirection.magnitude < 0.1f)
            {
                return false;
            }
        }

        return true;
    }

    protected override async Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
    {

        return effectGroup;
    }
}