using UGoap.Unity;
using UnityEngine;
using static UGoap.Base.PropertyManager;

[RequireComponent(typeof(UGoapAgent))]
public class TargetDetector : MonoBehaviour
{
    [SerializeField] private UGoapAgent _agent;
    [SerializeField] private string _target;
    [SerializeField] private float _range;

    private void Awake()
    {
        if (!_agent) _agent = GetComponent<UGoapAgent>();
    }
    
    private void Update()
    {
        UEntity entityPlayer = WorkingMemoryManager.Get(_target).Object;
        bool isNear = Vector3.Distance(entityPlayer.transform.position, transform.position) <= _range;
        bool previousIsNear = _agent.CurrentState.TryGetOrDefault(PropertyKey.PlayerNear, false);
            
        if (isNear != previousIsNear)
        {
            _agent.CurrentState.Set(PropertyKey.PlayerNear, isNear);
            if (isNear)
            {
                _agent.Interrupt();
            }
        }
    }
}