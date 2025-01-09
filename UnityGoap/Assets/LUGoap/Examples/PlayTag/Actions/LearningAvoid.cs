using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity.ScriptableObjects;
using System.Globalization;
using LUGoap.Learning;
using LUGoap.Unity;
using Random = LUGoap.Base.Random;

[CreateAssetMenu(fileName = "LearningAvoid", menuName = "LUGoap/Actions/PlayTag/LearningAvoid")]
public class LearningAvoid : LearningActionConfig<LearningAvoidAction>
{
    [Header("Custom Data")]
    public Vector2 XLimits;
    public Vector2 ZLimits;
    public int SpeedFactor = 1;
    
    protected override LearningAvoidAction Install(LearningAvoidAction action)
    {
        action.Init(XLimits, ZLimits, SpeedFactor);
        return action;
    }
}

public class LearningAvoidAction : LearningAction
{
    public Vector2 XLimits;
    public Vector2 ZLimits;
    public int SpeedFactor;

    public void Init(Vector2 xLimits, Vector2 zLimits, int speedFactor)
    {
        XLimits = xLimits;
        ZLimits = zLimits;
        SpeedFactor = speedFactor;
    }

    protected override string[] OnCreateParameters()
    {
        var parameters = new float[2];
        parameters[0] = Random.RangeToInt(XLimits.x, XLimits.y);
        parameters[1] = Random.RangeToInt(ZLimits.x, ZLimits.y);
        return SerializeParameters(parameters);
    }
    
    protected override Conditions GetProceduralConditions(ActionSettings settings)
    {
        return null;
    }

    protected override Effects GetProceduralEffects(ActionSettings settings)
    {
        Effects effects = new Effects();

        var parameters = DeSerializeParameters(settings.Parameters);
        
        effects.Set(PropertyManager.PropertyKey.DestinationX, BaseTypes.EffectType.Set, parameters[0]);
        effects.Set(PropertyManager.PropertyKey.DestinationZ, BaseTypes.EffectType.Set, parameters[1]);
        
        return effects;
    }

    protected override bool OnValidate(State nextState, IAgent iAgent, string[] parameters)
    {
        if (iAgent is not Agent agent) return false;

        if (iAgent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.IsIt, false)) return true;

        var args = DeSerializeParameters(parameters);
        
        var destination = new Vector3
        {
            x = args[0],
            z = args[1]
        };

        var destinationDirection = destination - agent.transform.position;
        destinationDirection.y = 0;
        if (destinationDirection.magnitude < 0.1f) return false;
        
        var targetX = iAgent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.TargetX, 0f);
        var targetZ = iAgent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.TargetZ, 0f);
        
        var targetPosition = new Vector3(targetX, destination.y, targetZ);
        
        var targetDirection = targetPosition - agent.transform.position;
        if (Vector3.Angle(destinationDirection, targetDirection) <= 45.0f)
        {
            return false;
        }

        return true;
    }

    protected override async Task<Effects> OnExecute(Effects effects, IAgent iAgent, string[] parameters, CancellationToken token)
    {
        if (iAgent is not Agent agent) return null;
        
        var x = (float)effects.TryGetOrDefault(PropertyManager.PropertyKey.DestinationX, 0f).Value;
        var z = (float)effects.TryGetOrDefault(PropertyManager.PropertyKey.DestinationZ, 0f).Value;
        var target = new Vector3(x, 0, z);
        
        var t = agent.transform;
        
        bool reached = false;

        var speed = agent.Speed * SpeedFactor;
        
        while (!reached)
        {
            if (token.IsCancellationRequested) return null;

            var p = t.position;
            target.y = p.y;
            t.position = Vector3.MoveTowards(p, target, speed * Time.deltaTime);
            t.rotation = Quaternion.LookRotation(target - p, Vector3.up);
            target.y = p.y;
            if (Vector3.Distance(t.position, target) < float.Epsilon)
            {
                reached = true;
            }
            await Task.Yield();
        }
        
        var isIt = iAgent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.IsIt, false);
        var playerNear = (string)iAgent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.PlayerNear) is "Close" or "Near";

        if (!isIt && !playerNear) return effects;
        return null;
    }
    
    //TODO: Parameters should be PropertyKeys type values, or something less generic.
    private string[] SerializeParameters(float[] parameters)
    {
        var result = new string[2];
        result[0] = parameters[0].ToString(CultureInfo.InvariantCulture);
        result[1] = parameters[1].ToString(CultureInfo.InvariantCulture);
        return result;
    }
    
    private float[] DeSerializeParameters(string[] parameters)
    {
        var result = new float[2];
        result[0] = float.Parse(parameters[0], CultureInfo.InvariantCulture);
        result[1] = float.Parse(parameters[1], CultureInfo.InvariantCulture);
        return result;
    }
}