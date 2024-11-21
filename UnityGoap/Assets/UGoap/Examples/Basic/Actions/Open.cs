using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity;
using UGoap.Unity.ScriptableObjects;
using static UGoap.Base.UGoapPropertyManager;

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

public class OpenAction : GoapAction
{
    public PropertyKey OpenState;
    public string Target;
    
    protected override GoapConditions GetProceduralConditions(GoapSettings settings)
    {
        return null;
    }
    
    protected override GoapEffects GetProceduralEffects(GoapSettings settings)
    {
        return null;
    }
    
    public override bool Validate(GoapState state, GoapActionInfo actionInfo, IGoapAgent iAgent)
    {
        if (iAgent is not UGoapAgent agent) return false;
        
        UGoapEntity entityDoor = UGoapWMM.Get(Target).Object;
        if (entityDoor.CurrentState.TryGetOrDefault(OpenState, "Opened") == "Locked")
        {
            state.Set(OpenState, "Locked");
            if (!state.TryGetOrDefault(PropertyKey.HasKey, false)) return false;
        }
        
        return true;
    }
    
    public override async Task<GoapState> Execute(GoapState state, IGoapAgent iAgent, CancellationToken token)
    {
        if (iAgent is not UGoapAgent agent) return null;
        
        UGoapEntity entityLocked = UGoapWMM.Get(Target).Object;
        entityLocked.GetComponent<Animator>()?.SetBool("Opened", true);
        return state;
    }
}