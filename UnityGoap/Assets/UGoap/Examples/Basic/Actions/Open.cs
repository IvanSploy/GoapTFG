using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity;
using UGoap.Unity.ScriptableObjects;
using static UGoap.Base.PropertyManager;

[CreateAssetMenu(fileName = "Open", menuName = "UGoap/Actions/Basic/Open")]
public class Open : ActionConfig<OpenAction>
{
    public PropertyKey OpenState;
    public string Target;
    
    protected override OpenAction Install(OpenAction action)
    {
        action.OpenState = OpenState;
        action.Target = Target;
        return action;
    }
}

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
        UEntity entityDoor = WorkingMemoryManager.Get(Target).Object;
        if (entityDoor.CurrentState.TryGetOrDefault(OpenState, "Opened") == "Locked")
        {
            iAgent.CurrentState.Set(OpenState, "Locked");
            if (!iAgent.CurrentState.TryGetOrDefault(PropertyKey.HasKey, false)) return false;
        }
        
        return true;
    }
    
    protected override async Task<State> OnExecute(State state, IAgent iAgent, string[] parameters, CancellationToken token)
    {
        UEntity entityLocked = WorkingMemoryManager.Get(Target).Object;
        var openBehaviour = entityLocked.GetComponent<OpenableBehaviour>();
        openBehaviour.Open();
        while (!openBehaviour.IsOpen)
        {
            await Task.Yield();
        }
        return state;
    }
}