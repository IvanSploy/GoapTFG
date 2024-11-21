using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity;
using UGoap.Unity.ScriptableObjects;

[CreateAssetMenu(fileName = "GoToDestination", menuName = "UGoap/Actions/PlayTag/GoToDestination")]
public class GoToDestination : ActionConfig<GoToDestinationAction>
{
    public int SpeedFactor = 1;
    protected override GoToDestinationAction Install(GoToDestinationAction action)
    {
        action.SpeedFactor = SpeedFactor;
        return action;
    }
}

public class GoToDestinationAction : GoapAction
{
    public int SpeedFactor = 1;
    
    protected override GoapConditions GetProceduralConditions(GoapSettings settings)
    {
        return null;
    }
    
    protected override GoapEffects GetProceduralEffects(GoapSettings settings)
    {
        return null;
    }
    
    public override bool Validate(GoapState state, GoapActionInfo actionInfo, IGoapAgent iAgent)
    {
        if (iAgent is not UGoapAgent agent) return false;
        
        return true;
    }

    public override async Task<GoapState> Execute(GoapState state, IGoapAgent iAgent, CancellationToken token)
    {
        if (iAgent is not UGoapAgent agent) return null;
        
        var x = state.TryGetOrDefault(UGoapPropertyManager.PropertyKey.DestinationX, 0f);
        var z = state.TryGetOrDefault(UGoapPropertyManager.PropertyKey.DestinationZ, 0f);
        var target = new Vector3(x, 0, z);
        
        var t = agent.transform;
        
        bool reached = false;

        var speed = agent.Speed * SpeedFactor;
        
        while (!reached)
        {
            if (token.IsCancellationRequested)
            {
                
                return state;
            }

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

        return state;
    }
}