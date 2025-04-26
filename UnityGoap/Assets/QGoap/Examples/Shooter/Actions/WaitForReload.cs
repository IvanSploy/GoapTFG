using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;
using QGoap.Unity;
using QGoap.Learning;
using Panda.Examples.Shooter;
using Random = QGoap.Base.Random;

public class WaitForReloadAction : LearningAction
{
    private GoapUnit _unit;
    
    protected override void Init()
    {
        if (_agent is not GoapAgent agent) return;
        _unit = agent.GetComponent<GoapUnit>();
    }
    
    protected override string[] OnCreateParameters(ActionSettings settings)
    {
        return new[]
        {
            Random.RangeToInt(1, _unit.startAmmo).ToString(),
        };
    }

    protected override EffectGroup GetProceduralEffects(ActionSettings settings)
    {
        var effects = new EffectGroup();
        effects.Set(PropertyManager.PropertyKey.Ammo, BaseTypes.EffectType.Set, int.Parse(settings.Parameters[0]));
        return effects;
    }

    protected override async Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
    {
        var waitAmmo = int.Parse(parameters[0]);
        while (_unit.ammo < waitAmmo) await Task.Yield();
        return effectGroup;
    }
}