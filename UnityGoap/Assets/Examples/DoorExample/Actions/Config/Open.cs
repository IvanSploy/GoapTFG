using UnityEngine;
using UGoap.Unity.ScriptableObjects;
using static UGoap.Base.UGoapPropertyManager;

[CreateAssetMenu(fileName = "Open", menuName = "Goap Items/Actions/Open")]
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