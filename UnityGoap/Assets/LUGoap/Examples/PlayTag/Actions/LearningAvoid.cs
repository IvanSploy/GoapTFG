using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using System.Globalization;
using LUGoap.Learning;
using LUGoap.Unity;
using Random = LUGoap.Base.Random;

public class LearningAvoidAction : LearningAction
{
    public Vector2 XLimits;
    public Vector2 ZLimits;
    public int SpeedFactor;

    private GoapAgent _goapAgent;
    private Transform _transform;

    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _goapAgent = agent;
        _transform = agent.transform;
    }

    protected override string[] OnCreateParameters(ActionSettings settings)
    {
        var parameters = new float[2];
        
        bool found;
        do
        {
            parameters[0] = Random.RangeToInt(XLimits.x, XLimits.y);
            parameters[1] = Random.RangeToInt(ZLimits.x, ZLimits.y);
            
            var destination = new Vector3
            {
                x = parameters[0],
                z = parameters[1],
            };
        
            var origin = new Vector3
            {
                x = settings.InitialState.TryGetOrDefault(PropertyManager.PropertyKey.DestinationX, 0f),
                z = settings.InitialState.TryGetOrDefault(PropertyManager.PropertyKey.DestinationZ, 0f)
            };

            var target = new Vector3
            {
                x = settings.InitialState.TryGetOrDefault(PropertyManager.PropertyKey.TargetX, 0f),
                z = settings.InitialState.TryGetOrDefault(PropertyManager.PropertyKey.TargetZ, 0f)
            };

            found = CheckDestination(origin, target, destination);
        } while (!found);
            
        return SerializeParameters(parameters);
    }

    public bool CheckDestination(Vector3 origin, Vector3 target, Vector3 destination)
    {
        var destinationDirection = destination - origin;
        destinationDirection.y = 0;
        if (destinationDirection.magnitude < 0.1f) return false;
        
        var targetDirection = target - origin;
        if(targetDirection == Vector3.zero) return true;
        return !(Vector3.Angle(destinationDirection, targetDirection) <= 45.0f);
    }

    protected override Effects GetProceduralEffects(ActionSettings settings)
    {
        Effects effects = new Effects();

        var parameters = DeSerializeParameters(settings.Parameters);
        
        effects.Set(PropertyManager.PropertyKey.DestinationX, BaseTypes.EffectType.Set, parameters[0]);
        effects.Set(PropertyManager.PropertyKey.DestinationZ, BaseTypes.EffectType.Set, parameters[1]);
        
        return effects;
    }

    protected override bool OnValidate(State nextState, string[] parameters)
    {
        if (_agent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.IsIt, false)) return true;

        var args = DeSerializeParameters(parameters);
        
        var destination = new Vector3
        {
            x = args[0],
            z = args[1]
        };

        var destinationDirection = destination - _transform.position;
        destinationDirection.y = 0;
        if (destinationDirection.magnitude < 0.1f) return false;
        
        var targetX = _agent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.TargetX, 0f);
        var targetZ = _agent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.TargetZ, 0f);
        
        var targetPosition = new Vector3(targetX, destination.y, targetZ);
        
        var targetDirection = targetPosition - _transform.position;
        return !(Vector3.Angle(destinationDirection, targetDirection) <= 45.0f);
    }

    protected override async Task<Effects> OnExecute(Effects effects, string[] parameters, CancellationToken token)
    {
        var x = (float)effects.TryGetOrDefault(PropertyManager.PropertyKey.DestinationX, 0f).Value;
        var z = (float)effects.TryGetOrDefault(PropertyManager.PropertyKey.DestinationZ, 0f).Value;
        var target = new Vector3(x, 0, z);
        
        bool reached = false;

        var speed = _goapAgent.Speed * SpeedFactor;
        
        while (!reached)
        {
            if (token.IsCancellationRequested) return null;

            var p = _transform.position;
            target.y = p.y;
            _transform.position = Vector3.MoveTowards(p, target, speed * Time.deltaTime);
            _transform.rotation = Quaternion.LookRotation(target - p, Vector3.up);
            target.y = p.y;
            if (Vector3.Distance(_transform.position, target) < float.Epsilon)
            {
                reached = true;
            }
            await Task.Yield();
        }
        
        var isIt = _agent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.IsIt, false);
        var playerNear = (string)_agent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.PlayerNear) is "Close" or "Near";

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