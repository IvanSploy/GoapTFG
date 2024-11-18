using UGoap.Base;
using UGoap.Unity.ScriptableObjects;
using UnityEngine;

namespace UGoap.Unity.Actions
{
    [CreateAssetMenu(fileName = "GenericAction", menuName = "Goap Items/Actions/GenericAction")]
    public class GenericActionConfig : ActionConfig
    {
        [SerializeField] private int _waitSeconds = 1;
        
        public override GoapAction CreateAction(GoapConditions conditions, GoapEffects effects)
        {
            var goapAction = new GenericAction();
            goapAction.Initialize(name, conditions, effects);
            goapAction.WaitSeconds = _waitSeconds;
            return goapAction;
        }
    }
}