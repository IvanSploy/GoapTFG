using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;

public static class WorkingMemoryManager
{
    public static WorkingMemory<Vector3, GameObject> WorkingMemory = new();
    
    //Blackboard Management
    public static void Add(GameObject go)
    {
        MemoryFact<Vector3, GameObject> mFact = new MemoryFact<Vector3, GameObject>
        {
            Name = go.name,
            Position = new()
            {
                Value = go.transform.position,
                Confidence = 1
            },
            Direction = new()
            {
                Value = go.transform.forward,
                Confidence = 1
            },
            Object = new()
            {
                Value = go,
                Confidence = 1
            },
            UpdateTime = 0
        };
        WorkingMemory.Add(mFact);
    }

    public static MemoryFact<Vector3, GameObject> Get(string name)
    {
        return WorkingMemory.Find(name);
    }
    
    public static MemoryFact<Vector3, GameObject> Get(GameObject go)
    {
        return WorkingMemory.Find(go);
    }
    
    public static void Update(string name, GameObject go)
    {
        WorkingMemory.Remove(name);
        Add(go);
    }
    
    public static void Update(GameObject go)
    {
        WorkingMemory.Remove(go);
        Add(go);
    }

    public static void Remove(GameObject go)
    {
        WorkingMemory.Remove(go);
    }
}
