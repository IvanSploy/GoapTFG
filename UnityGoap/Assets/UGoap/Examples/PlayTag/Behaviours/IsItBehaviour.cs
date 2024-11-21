using UGoap.Unity;
using UnityEngine;
using static UGoap.Base.UGoapPropertyManager;

[RequireComponent(typeof(UGoapAgent))]
public class IsItBehaviour : MonoBehaviour
{
    [SerializeField] private UGoapAgent _goapAgent;
    [SerializeField] private Renderer _renderer;

    private void Awake()
    {
        if (!_goapAgent) _goapAgent = GetComponent<UGoapAgent>();
        if(!_renderer) _renderer = GetComponentInChildren<Renderer>();
        var isIt = _goapAgent.CurrentState.TryGetOrDefault(PropertyKey.IsIt, false);
        Tag(isIt);
    }

    private void Tag(bool isIt)
    {
        _renderer.material.color = isIt ? Color.red : Color.cyan;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var isIt = _goapAgent.CurrentState.TryGetOrDefault(PropertyKey.IsIt, false);
        isIt = !isIt;
        _goapAgent.CurrentState.Set(PropertyKey.IsIt, isIt);
        Tag(isIt);
        Debug.Log($"[GOAP] {_goapAgent}COLLISION GOAP");
        _goapAgent.Interrupt();
    }
}