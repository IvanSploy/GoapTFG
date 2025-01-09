using LUGoap.Unity;
using UnityEngine;
using static LUGoap.Base.PropertyManager;

[RequireComponent(typeof(Agent))]
public class TargetProximityDetector : MonoBehaviour
{
    [SerializeField] private Agent _agent;
    [SerializeField] private string _target;
    [SerializeField] private float _closeRange;
    [SerializeField] private float _nearRange;

    private void Awake()
    {
        if (!_agent) _agent = GetComponent<Agent>();
    }
    
    private void Update()
    {
        UEntity entityPlayer = WorkingMemoryManager.Get(_target).Object;

        var nearState = "Close";

        if (Vector3.Distance(entityPlayer.transform.position, transform.position) > _closeRange)
        {
            nearState = "Near";
            if (Vector3.Distance(entityPlayer.transform.position, transform.position) > _nearRange)
            {
                nearState = "Far";
            }
        }
        
        var previousIsNear = (string)_agent.CurrentState.TryGetOrDefault(PropertyKey.PlayerNear);
        
        if (nearState != previousIsNear)
        {
            _agent.CurrentState.Set(PropertyKey.PlayerNear, nearState);
            if (nearState == "Near" && previousIsNear == "Far")
            {
                _agent.Interrupt();
            }
        }
    }
}