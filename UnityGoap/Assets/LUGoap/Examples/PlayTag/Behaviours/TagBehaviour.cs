using LUGoap.Unity;
using UnityEngine;
using static LUGoap.Base.PropertyManager;

[RequireComponent(typeof(Agent))]
public class TagBehaviour : MonoBehaviour, ITaggable
{
    [SerializeField] private Agent _agent;
    [SerializeField] private Renderer _renderer;
    
    private void Awake()
    {
        if (!_agent) _agent = GetComponent<Agent>();
        if(!_renderer) _renderer = GetComponentInChildren<Renderer>();
    }

    private void Start()
    {
        var isIt = _agent.CurrentState.TryGetOrDefault(PropertyKey.IsIt, false);
        UpdateColor(isIt);
    }
    
    public void Tag(float tagCooldown)
    {
        UpdateColor(true);
        _agent.CurrentState.Set(PropertyKey.IsIt, true);
        _agent.Interrupt(tagCooldown);
    }

    public void UnTag()
    {
        UpdateColor(false);
        _agent.CurrentState.Set(PropertyKey.IsIt, false);
        _agent.Interrupt();
    }

    private void UpdateColor(bool isIt)
    {
        _renderer.material.color = isIt ? Color.magenta : Color.cyan;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var isIt = _agent.CurrentState.TryGetOrDefault(PropertyKey.IsIt, false);
        if (isIt)
        {
            TagManager.Instance.Tag();
        }
    }
}