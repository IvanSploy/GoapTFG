using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    private readonly Queue<Checkpoint> _checkpoints = new();
    
    public int Count => _checkpoints.Count;

    private void Awake()
    {
        var checkpoints = GetComponentsInChildren<Checkpoint>();
        foreach (var checkpoint in checkpoints)
        {
            _checkpoints.Enqueue(checkpoint);
        }
    }

    public void ResetAll()
    {
        foreach (var checkpoint in _checkpoints)
        {
            checkpoint.Reset();
        }
    }
    
    public bool IsEmpty() => _checkpoints.Count == 0;

    public Vector3 GetCurrent() => _checkpoints.Peek().transform.position;
    
    public void Next()
    {
        if (IsEmpty()) return;
        _checkpoints.Dequeue();
    }
}
