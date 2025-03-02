using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;
using Panda.Examples.Shooter;

public class FireAction : Action
{
    private GoapUnit _unit;
    private GoapAI _ai;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _unit = agent.GetComponent<GoapUnit>();
        _ai = agent.GetComponent<GoapAI>();
    }

    protected override ConditionGroup GetProceduralConditions(ActionSettings settings)
    {
        var conditions = new ConditionGroup();
        conditions.SetOrCombine(PropertyManager.PropertyKey.Ammo, BaseTypes.ConditionType.GreaterThan, 0);
        return conditions;
    }

    protected override EffectGroup GetProceduralEffects(ActionSettings settings)
    {
        var effects = new EffectGroup();
        effects.Set(PropertyManager.PropertyKey.Ammo, BaseTypes.EffectType.Subtract, 1);
        return effects;
    }

    protected override async Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
    {
        _ai.SetTarget_Enemy();
        await _unit.AimAt_Target(token);
        _unit.Fire();
        return effectGroup;
    }
}