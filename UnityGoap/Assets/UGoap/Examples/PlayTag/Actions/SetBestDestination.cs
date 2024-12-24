using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity;
using UGoap.Unity.ScriptableObjects;
using System.Globalization;
using UGoap.Learning;

[CreateAssetMenu(fileName = "SetBestDestination", menuName = "UGoap/Actions/PlayTag/SetBestDestination")]
public class SetBestDestination : LearningActionConfig<SetBestDestinationAction>
{
    public Vector2 XLimits;
    public Vector2 ZLimits;
    
    protected override SetBestDestinationAction Install(SetBestDestinationAction action)
    {
        action.Init(XLimits, ZLimits);
        return action;
    }
}

public class SetBestDestinationAction : LearningAction
{
    public Vector2 XLimits;
    public Vector2 ZLimits;

    public void Init(Vector2 xLimits, Vector2 zLimits)
    {
        XLimits = xLimits;
        ZLimits = zLimits;
    }

    protected override string[] OnCreateParameters()
    {
        var parameters = new string[2];
        parameters[0] = Mathf.RoundToInt(Random.Range(XLimits.x, XLimits.y)).ToString();
        parameters[1] = Mathf.RoundToInt(Random.Range(ZLimits.x, ZLimits.y)).ToString();
        return parameters;
    }
    
    protected override Conditions GetProceduralConditions(ActionSettings settings)
    {
        return null;
    }

    protected override Effects GetProceduralEffects(ActionSettings settings)
    {
        Effects effects = new Effects();
        effects.Set(PropertyManager.PropertyKey.DestinationX, BaseTypes.EffectType.Set, 0f);
        effects.Set(PropertyManager.PropertyKey.DestinationZ, BaseTypes.EffectType.Set, 0f);
        return effects;
    }

    protected override bool OnValidate(State nextState, IAgent iAgent, string[] parameters)
    {
        if (iAgent is not UGoapAgent agent) return false;

        if (iAgent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.IsIt, false)) return true;
        var destination = new Vector3
        {
            x = float.Parse(parameters[0], CultureInfo.InvariantCulture),
            z = float.Parse(parameters[1], CultureInfo.InvariantCulture)
        };

        var destinationDirection = destination - agent.transform.position;
        destinationDirection.y = 0;
        if (destinationDirection.magnitude < 0.1f) return false;

        return true;
    }

    protected override async Task<State> OnExecute(State nextState, IAgent iAgent, string[] parameters, CancellationToken token)
    {
        if (iAgent is not UGoapAgent agent) return null;
        
        return nextState;
    }
}