using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;
using Panda.Examples.Shooter;

public class AcquireEnemy : Action
{
    private GoapAI _ai;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _ai = agent.GetComponent<GoapAI>();
    }
    
    protected override bool OnValidate(State state, string[] parameters)
    {
        if(_ai.HasEnemy()) return true;
        _agent.CurrentState.Set(PropertyManager.PropertyKey.HasMoved, false);
        return false;
    }

    protected override Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
    {
        return Task.FromResult(effectGroup);
    }
}