using QGoap.Unity;
using UnityEngine;
using UnityEngine.Serialization;
using static QGoap.Base.PropertyManager;

[RequireComponent(typeof(GoapAgent))]
public class TagBehaviour : MonoBehaviour, ITaggable
{
    [FormerlySerializedAs("_agent")] [SerializeField] private GoapAgent _goapAgent;
    [SerializeField] private Renderer _renderer;
    
    private void Awake()
    {
        if (!_goapAgent) _goapAgent = GetComponent<GoapAgent>();
        if(!_renderer) _renderer = GetComponentInChildren<Renderer>();
    }

    private void Start()
    {
        var isIt = _goapAgent.CurrentState.TryGetOrDefault(PropertyKey.IsIt, false);
        UpdateColor(isIt);
    }
    
    public void Tag(float tagCooldown)
    {
        UpdateColor(true);
        _goapAgent.CurrentState.Set(PropertyKey.IsIt, true);
        _goapAgent.ForceInterrupt(tagCooldown);
    }

    public void UnTag()
    {
        UpdateColor(false);
        _goapAgent.CurrentState.Set(PropertyKey.IsIt, false);
        _goapAgent.Interrupt();
    }

    private void UpdateColor(bool isIt)
    {
        _renderer.material.color = isIt ? Color.magenta : Color.cyan;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        var isIt = _goapAgent.CurrentState.TryGetOrDefault(PropertyKey.IsIt, false);
        if (isIt)
        {
            TagManager.Instance.Tag();
        }
    }
}