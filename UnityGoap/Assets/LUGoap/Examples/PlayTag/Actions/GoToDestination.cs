using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;
using Action = LUGoap.Base.Action;

public class GoToDestinationAction : Action
{
    public int SpeedFactor = 1;

    protected override Conditions GetProceduralConditions(ActionSettings settings)
    {
        return null;
    }
    
    protected override Effects GetProceduralEffects(ActionSettings settings)
    {
        return null;
    }
    
    protected override bool OnValidate(State nextState, IAgent iAgent, string[] parameters)
    {
        if (iAgent is not GoapAgent agent) return false;
        
        return true;
    }

    protected override async Task<Effects> OnExecute(Effects effects, IAgent iAgent, string[] parameters, CancellationToken token)
    {
        if (iAgent is not GoapAgent agent) return null;
        
        var x = iAgent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.DestinationX, 0f);
        var z = iAgent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.DestinationZ, 0f);
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

        return effects;
    }
}