using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using Unity.VisualScripting;
using UnityEngine;

namespace GoapTFG.Unity
{
    public static class WorkingMemoryManager
    {
        private static WorkingMemory<Vector3, GoapEntity> WMemory = new();
        
        //Blackboard Management
        public static void Add(string name, GoapEntity ge)
        {
            MemoryFact<Vector3, GoapEntity> mFact = new MemoryFact<Vector3, GoapEntity>
            {
                Name = name,
                Position = ge.transform.position,
                Direction = ge.transform.forward,
                Object = ge
            };
            WMemory.Add(mFact);
        }

        public static MemoryFact<Vector3, GoapEntity> Get(string name)
        {
            return WMemory.Find(name);
        }
        
        public static MemoryFact<Vector3, GoapEntity> Get(GoapEntity ge)
        {
            return WMemory.Find(ge);
        }
        
        public static void Update(string name, GoapEntity ge)
        {
            WMemory.Remove(name);
            Add(name, ge);
        }
        
        public static void Update(GoapEntity ge)
        {
            WMemory.Remove(ge);
            Add(ge.Name, ge);
        }

        public static void Remove(GoapEntity ge)
        {
            WMemory.Remove(ge);
        }
    }
}
