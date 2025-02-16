using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;
using Panda.Examples.Shooter;

public class MoveToRandomAction : Action
{
    public float Range;
    
    private GoapUnit _unit;
    private GoapAI _ai;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _unit = agent.GetComponent<GoapUnit>();
        _ai = agent.GetComponent<GoapAI>();
    }
    
    protected override async Task<Effects> OnExecute(Effects effects, string[] parameters, CancellationToken token)
    {
        _ai.SetDestination_Random(Range);
        _unit.Move();
        await _unit.WaitArrival(token);
        return effects;
    }
}