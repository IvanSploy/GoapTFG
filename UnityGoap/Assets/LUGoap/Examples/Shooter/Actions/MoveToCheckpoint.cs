using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;
using Panda.Examples.Shooter;

public class MoveToCheckpointAction : Action
{
    private GoapGoalSeeker _goalSeeker;
    private GoapUnit _unit;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _goalSeeker = agent.GetComponent<GoapGoalSeeker>();
        _unit = agent.GetComponent<GoapUnit>();
    }
    
    protected override async Task<Effects> OnExecute(Effects effects, string[] parameters, CancellationToken token)
    {
        if (!_goalSeeker.SetDestination_CheckPoint()) return null;
        _unit.Move();
        await _unit.WaitArrival(token);
        return effects;
    }
}