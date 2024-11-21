using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity;
using UGoap.Unity.ScriptableObjects;
using UnityEngine;


[CreateAssetMenu(fileName = "SetDestinationCover", menuName = "UGoap/Actions/Shooter/SetDestinationCover")]
public class SetDestinationCover : ActionConfig<SetDestinationCoverAction>
{
    protected override SetDestinationCoverAction Install(SetDestinationCoverAction action)
    {
        return action;
    }
}

public class SetDestinationCoverAction : GoapAction
{
    protected override GoapConditions GetProceduralConditions(GoapSettings settings)
    {
        return null;
    }

    protected override GoapEffects GetProceduralEffects(GoapSettings settings)
    {
        return null;
    }

    public override bool Validate(GoapState goapState, GoapActionInfo actionInfo, IGoapAgent iAgent)
    {
        if (iAgent is not UGoapAgent agent) return false;

        return true;
    }

    public override async Task<GoapState> Execute(GoapState goapState, IGoapAgent iAgent, CancellationToken token)
    {
        if (iAgent is not UGoapAgent agent) return null;

        return goapState;
    }
}