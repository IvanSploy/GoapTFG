using System;
using System.Threading;
using System.Threading.Tasks;
using QGoap.Base;
using QGoap.Unity;
using static QGoap.Base.PropertyManager;
using Action = QGoap.Base.Action;

[Serializable]
public class OpenAction : Action
{
    public PropertyKey OpenState;
    public string Target;

    protected override void Init() { }

    protected override bool OnValidate(State nextState, string[] parameters)
    {
        GoapEntity entityDoor = WorkingMemoryManager.Get(Target).Object;
        if (entityDoor.CurrentState.TryGetOrDefault(OpenState, "Opened") == "Locked")
        {
            _agent.CurrentState.Set(OpenState, "Locked");
            if (!_agent.CurrentState.TryGetOrDefault(PropertyKey.HasKey, false)) return false;
        }
        
        return true;
    }
    
    protected override async Task<EffectGroup> OnExecute(EffectGroup effectGroup, string[] parameters, CancellationToken token)
    {
        GoapEntity entityLocked = WorkingMemoryManager.Get(Target).Object;
        var openBehaviour = entityLocked.GetComponent<OpenableBehaviour>();
        openBehaviour.Open();
        while (!openBehaviour.IsOpen) await Task.Yield();
        return effectGroup;
    }
}