using System.Globalization;
using UGoap.Base;
using UGoap.Learning;
using UnityEngine;
using UnityEngine.Serialization;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "SetBestDestination", menuName = "Goap Items/Actions/SetBestDestination")]
    public class SetBestDestinationAction : UGoapAction
    {
        [SerializeField] private bool _active = true;
        [SerializeField] private LearningConfig _learningConfig;
        [SerializeField] private string _name;
        
        protected override GoapConditions GetProceduralConditions(GoapSettings settings)
        {
            return null;
        }

        protected override GoapEffects GetProceduralEffects(GoapSettings settings)
        {
            if (_active)
            {
                GoapEffects goapEffects = new GoapEffects();
                var learningState = _learningConfig.GetLearningStateCode(settings.InitialState, settings.Goal);
                var bestActionName = _learningConfig.FindMax(learningState, _name);

                if (bestActionName != null)
                {
                    var split = bestActionName.Split("_");
                    goapEffects.Set(PropertyKey.DestinationX, BaseTypes.EffectType.Set, float.Parse(split[1], NumberStyles.Any));
                    goapEffects.Set(PropertyKey.DestinationZ, BaseTypes.EffectType.Set, float.Parse(split[2], NumberStyles.Any));
                    return goapEffects;
                }
            }

            return null;
        }

        public override string GetName(GoapConditions conditions, GoapEffects effects)
        {
            string actionName = _name;
            actionName += effects.TryGetOrDefault(PropertyKey.DestinationX, 0f).Value + "_";
            actionName += effects.TryGetOrDefault(PropertyKey.DestinationZ, 0f).Value;
            return actionName;
        }

        public override bool ProceduralValidate(GoapState goapState, GoapActionInfo actionInfo, UGoapAgent agent)
        {
            if (!goapState.TryGetOrDefault(PropertyKey.IsIt, false))
            {
                    var playerPosition = UGoapWMM.Get("Player").Object.transform.position;
                    var destination = playerPosition;
                    destination.x = (float)actionInfo.Effects.TryGetOrDefault(PropertyKey.DestinationX, 0f).Value;
                    destination.z = (float)actionInfo.Effects.TryGetOrDefault(PropertyKey.DestinationZ, 0f).Value;

                    var destinationDirection = destination - agent.transform.position;
                    if (destinationDirection.magnitude < 0.1f)
                    {
                        return false;
                    }
                    
                    var playerDirection = playerPosition - agent.transform.position;
                    if (playerDirection.magnitude > 0.1f && Vector3.Angle(destinationDirection, playerDirection) <= 45.0f)
                    {
                        return false;
                    }
            }

            return true;
        }

        public override void ProceduralExecute(ref GoapState goapState, UGoapAgent agent)
        {
            agent.GoGenericAction(Name, ref goapState);
        }
    }
}