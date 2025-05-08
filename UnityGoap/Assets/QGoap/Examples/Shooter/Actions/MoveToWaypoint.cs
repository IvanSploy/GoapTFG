using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;
using QGoap.Unity;
using Panda.Examples.Shooter;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;

public class MoveToWaypointAction : Action
{
    public bool Previous = true;

    private WaypointManager _waypointManager;
    private GoapAI _ai;
    private GoapUnit _unit;
    private NavMeshAgent _navMeshAgent;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _waypointManager = Object.FindAnyObjectByType<WaypointManager>();
        _ai = agent.GetComponent<GoapAI>();
        _unit = agent.GetComponent<GoapUnit>();
        _navMeshAgent = agent.GetComponent<NavMeshAgent>();
    }

    protected override async Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
    {
        var (previous, next) = _waypointManager.GetWaypoints(_navMeshAgent);
        _ai.SetDestination(Previous ? previous : next);
        _unit.Move();
        await _unit.WaitArrival(token);
        return effectGroup;
    }
}