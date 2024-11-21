using UnityEngine;
using System.Threading;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Unity;
using UGoap.Unity.ScriptableObjects;
using UGoap.Learning;
using System.Globalization;

[CreateAssetMenu(fileName = "SetBestDestination", menuName = "UGoap/Actions/PlayTag/SetBestDestination")]
public class SetBestDestination : ActionConfig<SetBestDestinationAction>
{
    public LearningConfig LearningConfig;
    
    protected override SetBestDestinationAction Install(SetBestDestinationAction action)
    {
        action.LearningConfig = LearningConfig;
        return action;
    }
}

public class SetBestDestinationAction : GoapAction
{
    //TODO: Implementar aprendizaje local.
    public LearningConfig LearningConfig;
    
    protected override GoapConditions GetProceduralConditions(GoapSettings settings)
    {
        return null;
    }

    protected override GoapEffects GetProceduralEffects(GoapSettings settings)
    {
        GoapEffects goapEffects = new GoapEffects();
        var learningState = LearningConfig.GetLearningStateCode(settings.InitialState, settings.Goal);
        var bestActionName = LearningConfig.FindMax(learningState, LearningConfig.name);

        if (bestActionName != null)
        {
            var split = bestActionName.Split("_");
            goapEffects.Set(UGoapPropertyManager.PropertyKey.DestinationX, BaseTypes.EffectType.Set, float.Parse(split[1], NumberStyles.Any));
            goapEffects.Set(UGoapPropertyManager.PropertyKey.DestinationZ, BaseTypes.EffectType.Set, float.Parse(split[2], NumberStyles.Any));
            return goapEffects;
        }

        return null;
    }

    public override bool Validate(GoapState goapState, GoapActionInfo actionInfo, IGoapAgent iAgent)
    {
        if (iAgent is not UGoapAgent goapAgent) return false;
        
        if (!goapState.TryGetOrDefault(UGoapPropertyManager.PropertyKey.IsIt, false))
        {
            var playerPosition = UGoapWMM.Get("Player").Object.transform.position;
            var destination = playerPosition;
            destination.x = (float)actionInfo.Effects.TryGetOrDefault(UGoapPropertyManager.PropertyKey.DestinationX, 0f).Value;
            destination.z = (float)actionInfo.Effects.TryGetOrDefault(UGoapPropertyManager.PropertyKey.DestinationZ, 0f).Value;

            var destinationDirection = destination - goapAgent.transform.position;
            if (destinationDirection.magnitude < 0.1f)
            {
                return false;
            }
            
            var playerDirection = playerPosition - goapAgent.transform.position;
            if (playerDirection.magnitude > 0.1f && Vector3.Angle(destinationDirection, playerDirection) <= 45.0f)
            {
                return false;
            }
        }

        return true;
    }

    public override async Task<GoapState> Execute(GoapState state, IGoapAgent iAgent, CancellationToken token)
    {
        if (iAgent is not UGoapAgent goapAgent) return null;
        
        return state;
    }
}