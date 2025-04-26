using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;
using QGoap.Unity;
using QGoap.Learning;
using Panda.Examples.Shooter;

public class SetTargetAction : LearningAction
{
    private GoapAI _ai;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _ai = agent.GetComponent<GoapAI>();
    }
    
    protected override string[] OnCreateParameters(ActionSettings settings)
    {
        return new[] { $"{Random.RangeToInt(-179, 180)}" };
    }
    
    protected override Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
    {
        _ai.SetGlobalTarget_Angle(float.Parse(parameters[0]));
        return Task.FromResult(effectGroup);
    }
}