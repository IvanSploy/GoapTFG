using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;
using QGoap.Unity;
using Panda.Examples.Shooter;

public class MoveToRandomAction : Action
{
    public float MinDistance;
    public float MaxDistance;
    public float Offset;
    
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
        if (!_ai.SetDestination_Random(MinDistance, MaxDistance, Offset)) return null;
        _unit.Move();
        await _unit.WaitArrival(token);
        return effectGroup;
    }
}