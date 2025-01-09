using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;
using LUGoap.Unity.ScriptableObjects;

[CreateAssetMenu(fileName = "PickUp", menuName = "LUGoap/Actions/Basic/PickUp")]
public class PickUp : ActionConfig<PickUpAction>
{
    public string Target;
    
    protected override PickUpAction Install(PickUpAction action)
    {
        action.Target = Target;
        return action;
    }
}

public class PickUpAction : Action
{
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
        return true;
    }
    
    protected override async Task<Effects> OnExecute(Effects effects, IAgent iAgent, string[] parameters, CancellationToken token)
    {
        UEntity entityKey = WorkingMemoryManager.Get(Target).Object;
        Object.Destroy(entityKey.gameObject);
        
        return effects;
    }
}