using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity;
using UGoap.Unity.ScriptableObjects;
using static UGoap.Base.UGoapPropertyManager;

[CreateAssetMenu(fileName = "Tag", menuName = "UGoap/Actions/PlayTag/Tag")]
public class Tag : ActionConfig<TagAction>
{
    protected override TagAction Install(TagAction action)
    {
        return action;
    }
}

public class TagAction : GoapAction
{
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

        var isIt = state.TryGetOrDefault(PropertyKey.IsIt, true);
        if (isIt)
        {
            return false;
        }

        return true;
    }

    public override async Task<GoapState> Execute(GoapState state, IGoapAgent iAgent, CancellationToken token)
    {
        if (iAgent is not UGoapAgent agent) return null;
        return state;
    }
}