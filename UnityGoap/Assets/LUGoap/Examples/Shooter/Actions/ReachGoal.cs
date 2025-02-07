using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;

public class ReachGoalAction : Action
{
    private GoalSeeker _goalSeeker;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _goalSeeker = agent.GetComponent<GoalSeeker>();
    }
    
    protected override async Task<Effects> OnExecute(Effects effects, string[] parameters, CancellationToken token)
    {
        _goalSeeker.RemoveCheckPoint();
        return effects;
    }
}