using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;
using QGoap.Unity;
using Panda.Examples.Shooter;

public class AimAtTargetAction : Action
{
    private GoapUnit _unit;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _unit = agent.GetComponent<GoapUnit>();
    }
    
    protected override async Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
    {
        await _unit.AimAt_Target(token);
        return effectGroup;
    }
}