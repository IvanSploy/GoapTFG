using UnityEngine;
using UGoap.Unity.ScriptableObjects;

[CreateAssetMenu(fileName = "PickUp", menuName = "Goap Items/Actions/PickUp")]
public class PickUp : ActionConfig<PickUpAction>
{
    public string Target;
    
    protected override PickUpAction Install(PickUpAction action)
    {
        action.Target = Target;
        return action;
    }
}