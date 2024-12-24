using UGoap.Unity;
using UnityEngine;
using static UGoap.Base.PropertyManager;

[RequireComponent(typeof(UGoapAgent))]
public class IsItBehaviour : MonoBehaviour
{
    [SerializeField] private UGoapAgent _agent;
    [SerializeField] private Renderer _renderer;

    private void Awake()
    {
        if (!_agent) _agent = GetComponent<UGoapAgent>();
        if(!_renderer) _renderer = GetComponentInChildren<Renderer>();
        var isIt = _agent.CurrentState.TryGetOrDefault(PropertyKey.IsIt, false);
        Tag(isIt);
    }

    private void Tag(bool isIt)
    {
        _renderer.material.color = isIt ? Color.red : Color.cyan;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var isIt = _agent.CurrentState.TryGetOrDefault(PropertyKey.IsIt, false);
        isIt = !isIt;
        _agent.CurrentState.Set(PropertyKey.IsIt, isIt);
        Tag(isIt);
        Debug.Log($"[GOAP] {_agent}COLLISION GOAP");
        _agent.Interrupt();
    }
}