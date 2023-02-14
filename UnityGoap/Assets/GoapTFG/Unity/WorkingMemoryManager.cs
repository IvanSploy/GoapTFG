using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;

namespace GoapTFG.Unity
{
    public static class WorkingMemoryManager
    {
        private static WorkingMemory<Vector3, GameObject> WMemory = new();
        
        //Blackboard Management
        public static void Add(GameObject go)
        {
            MemoryFact<Vector3, GameObject> mFact = new MemoryFact<Vector3, GameObject>
            {
                Name = go.name,
                Position = go.transform.position,
                Direction = go.transform.forward,
                Object = go
            };
            WMemory.Add(mFact);
        }

        public static MemoryFact<Vector3, GameObject> Get(string name)
        {
            return WMemory.Find(name);
        }
        
        public static MemoryFact<Vector3, GameObject> Get(GameObject go)
        {
            return WMemory.Find(go);
        }
        
        public static void Update(string name, GameObject go)
        {
            WMemory.Remove(name);
            Add(go);
        }
        
        public static void Update(GameObject go)
        {
            WMemory.Remove(go);
            Add(go);
        }

        public static void Remove(GameObject go)
        {
            WMemory.Remove(go);
        }
    }
}
