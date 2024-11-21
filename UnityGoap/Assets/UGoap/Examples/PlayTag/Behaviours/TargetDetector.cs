using UGoap.Unity;
using UnityEngine;
using static UGoap.Base.UGoapPropertyManager;

[RequireComponent(typeof(UGoapAgent))]
public class TargetDetector : MonoBehaviour
{
    [SerializeField] private UGoapAgent _goapAgent;
    [SerializeField] private string _target;
    [SerializeField] private float _range;

    private void Awake()
    {
        if (!_goapAgent) _goapAgent = GetComponent<UGoapAgent>();
    }
    
    private void Update()
    {
        UGoapEntity entityPlayer = UGoapWMM.Get(_target).Object;
        bool isNear = Vector3.Distance(entityPlayer.transform.position, transform.position) <= _range;
        bool previousIsNear = _goapAgent.CurrentState.TryGetOrDefault(PropertyKey.PlayerNear, false);
            
        if (isNear != previousIsNear)
        {
            _goapAgent.CurrentState.Set(PropertyKey.PlayerNear, isNear);
            if (isNear)
            {
                _goapAgent.Interrupt();
            }
        }
    }
}