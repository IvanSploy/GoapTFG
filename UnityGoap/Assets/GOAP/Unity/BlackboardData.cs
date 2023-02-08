using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;

public static class BlackboardData
{
    public static Blackboard<Vector3, GameObject> blackboard = new();
    
    //Blackboard Management
    public static void Add(GameObject go)
    {
        MemoryFact<Vector3, GameObject> mFact = new MemoryFact<Vector3, GameObject>
        {
            Name = go.name,
            Position = go.transform.position,
            Direction = go.transform.forward,
            Object = go,
            UpdateTime = Time.time
        };
        blackboard.Add(mFact);
    }

    public static MemoryFact<Vector3, GameObject> Get(string name)
    {
        return blackboard.Find(name);
    }
    
    public static MemoryFact<Vector3, GameObject> Get(GameObject go)
    {
        return blackboard.Find(go);
    }
    
    public static void Update(string name, GameObject go)
    {
        blackboard.Remove(name);
        Add(go);
    }
    
    public static void Update(GameObject go)
    {
        blackboard.Remove(go);
        Add(go);
    }

    public static void Remove(GameObject go)
    {
        blackboard.Remove(go);
    }
}
