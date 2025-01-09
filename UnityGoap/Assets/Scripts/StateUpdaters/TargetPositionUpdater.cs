using LUGoap.Base;
using UnityEngine;
using static LUGoap.Base.PropertyManager;

[RequireComponent(typeof(IAgent))]
public class TargetPositionUpdater : MonoBehaviour
{
    public GameObject Target;

    public PropertyKey PropertyX;
    public PropertyKey PropertyY;
    public PropertyKey PropertyZ;

    private IAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<IAgent>();
    }

    private void Update()
    {
        UpdateTarget();
    }

    private void UpdateTarget()
    {
        var position = Target.transform.position;
        var x = Mathf.Round(position.x);
        var y = Mathf.Round(position.y);
        var z = Mathf.Round(position.z);
        
        if(PropertyX != PropertyKey.None) _agent.CurrentState.Set(PropertyX, x);
        if(PropertyY != PropertyKey.None) _agent.CurrentState.Set(PropertyY, y);
        if(PropertyZ != PropertyKey.None) _agent.CurrentState.Set(PropertyZ, z);
    }
}
