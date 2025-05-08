using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;
using QGoap.Unity;
using Panda.Examples.Shooter;

public class MoveToCoverAction : Action
{
    public float SearchRadius = 3;
    
    private GoapUnit _unit;
    private GoapAI _ai;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _unit = agent.GetComponent<GoapUnit>();
        _ai = agent.GetComponent<GoapAI>();
    }

    protected override async Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
    {
        if (!_ai.SetDestination_Cover(SearchRadius)) return null;
        _unit.Move();
        await _unit.WaitArrival(token);
        return effectGroup;
    }
}