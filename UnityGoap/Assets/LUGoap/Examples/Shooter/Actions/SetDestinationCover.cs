using System.Threading;
using System.Threading.Tasks;
using LUGoap.Base;
using LUGoap.Unity;
using LUGoap.Unity.ScriptableObjects;
using UnityEngine;


[CreateAssetMenu(fileName = "SetDestinationCover", menuName = "LUGoap/Actions/Shooter/SetDestinationCover")]
public class SetDestinationCover : ActionConfig<SetDestinationCoverAction>
{
    protected override SetDestinationCoverAction Install(SetDestinationCoverAction action)
    {
        return action;
    }
}

public class SetDestinationCoverAction : Action
{
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
        if (iAgent is not GoapAgent agent) return false;

        return true;
    }

    protected override async Task<Effects> OnExecute(Effects effects, IAgent iAgent, string[] parameters, CancellationToken token)
    {
        if (iAgent is not GoapAgent agent) return null;

        return effects;
    }
}