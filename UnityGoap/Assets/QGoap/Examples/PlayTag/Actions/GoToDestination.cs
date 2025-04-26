using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;
using Action = LUGoap.Base.Action;

public class GoToDestinationAction : Action
{
    public int SpeedFactor = 1;

    private GoapAgent _goapAgent;
    private Transform _transform;

    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _goapAgent = agent;
        _transform = _goapAgent.transform;
    }

    protected override async Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
    {
        var x = _agent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.DestinationX, 0f);
        var z = _agent.CurrentState.TryGetOrDefault(PropertyManager.PropertyKey.DestinationZ, 0f);
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

        return effectGroup;
    }
}