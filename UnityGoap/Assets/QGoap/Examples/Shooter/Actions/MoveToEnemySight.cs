using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;
using QGoap.Unity;

public class MoveToEnemySightAction : Action
{
    public int Tries;
    
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
        if (!_ai.Enemy) return null;
        int tries = 0;
        while (!_ai.IsEnemyInSight())
        {
            _ai.SetDestination_Random(0, (_ai.Enemy.transform.position - _unit.destination).magnitude, 5);
            tries++;
            if (tries >= Tries) return null;
        }
        _unit.Move();
        await _unit.WaitArrival(token);
        return effectGroup;
    }
}