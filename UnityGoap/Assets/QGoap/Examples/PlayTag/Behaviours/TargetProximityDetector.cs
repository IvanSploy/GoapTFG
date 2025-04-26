using QGoap.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using static QGoap.Base.PropertyManager;

[RequireComponent(typeof(GoapAgent))]
public class TargetProximityDetector : MonoBehaviour
{
    [FormerlySerializedAs("_agent")] [SerializeField] private GoapAgent _goapAgent;
    [SerializeField] private string _target;
    [SerializeField] private float _closeRange;
    [SerializeField] private float _nearRange;

    private void Awake()
    {
        if (!_goapAgent) _goapAgent = GetComponent<GoapAgent>();
    }
    
    private void Update()
    {
        GoapEntity entityPlayer = WorkingMemoryManager.Get(_target).Object;

        var nearState = "Close";

        if (Vector3.Distance(entityPlayer.transform.position, transform.position) > _closeRange)
        {
            nearState = "Near";
            if (Vector3.Distance(entityPlayer.transform.position, transform.position) > _nearRange)
            {
                nearState = "Far";
            }
        }
        
        var previousIsNear = (string)_goapAgent.CurrentState.TryGetOrDefault(PropertyKey.PlayerNear);
        
        if (nearState != previousIsNear)
        {
            _goapAgent.CurrentState.Set(PropertyKey.PlayerNear, nearState);
            if (nearState == "Near" && previousIsNear == "Far")
            {
                _goapAgent.Interrupt();
            }
        }
    }
}