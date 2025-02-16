using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    private CheckpointManager _manager;
    private bool _triggered;

    private void Awake()
    {
        _manager = GetComponentInParent<CheckpointManager>();
    }

    public void Reset()
    {
        _triggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_triggered) return;
        if (!other.gameObject.CompareTag("Player")) return;
        _triggered = true;
        _manager.Next();
    }
}
