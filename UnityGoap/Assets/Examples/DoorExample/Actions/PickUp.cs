using System.Threading;
using UnityEngine;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity;
using static UGoap.Base.UGoapPropertyManager;

public class PickUpAction : GoapAction
{
    public string Target;
    
    protected override GoapConditions GetProceduralConditions(GoapSettings settings)
    {
        return null;
    }
    
    protected override GoapEffects GetProceduralEffects(GoapSettings settings)
    {
        return null;
    }
    
    public override bool Validate(ref GoapState state, GoapActionInfo actionInfo, IGoapAgent iAgent)
    {
        if (iAgent is not UGoapAgent agent) return false;
        
        return true;
    }
    
    public override async Task<GoapState> Execute(GoapState state, IGoapAgent iAgent, CancellationToken token)
    {
        if (iAgent is not UGoapAgent agent) return null;
        
        UGoapEntity entityKey = UGoapWMM.Get(Target).Object;
        Object.Destroy(entityKey.gameObject);
        
        return state;
    }
}