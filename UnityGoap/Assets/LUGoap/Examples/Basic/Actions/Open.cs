using System;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;
using static LUGoap.Base.PropertyManager;
using Action = LUGoap.Base.Action;

[Serializable]
public class OpenAction : Action
{
    public PropertyKey OpenState;
    public string Target;

    protected override Conditions GetProceduralConditions(ActionSettings settings)
    {
        return null;
    }
    
    protected override Effects GetProceduralEffects(ActionSettings settings)
    {
        return null;
    }
    
    protected override bool OnValidate(State nextState, IAgent iAgent, string[] parameters)
    {
        GoapEntity entityDoor = WorkingMemoryManager.Get(Target).Object;
        if (entityDoor.CurrentState.TryGetOrDefault(OpenState, "Opened") == "Locked")
        {
            iAgent.CurrentState.Set(OpenState, "Locked");
            if (!iAgent.CurrentState.TryGetOrDefault(PropertyKey.HasKey, false)) return false;
        }
        
        return true;
    }
    
    protected override async Task<Effects> OnExecute(Effects effects, IAgent iAgent, string[] parameters, CancellationToken token)
    {
        GoapEntity entityLocked = WorkingMemoryManager.Get(Target).Object;
        var openBehaviour = entityLocked.GetComponent<OpenableBehaviour>();
        openBehaviour.Open();
        while (!openBehaviour.IsOpen) await Task.Yield();
        return effects;
    }
}