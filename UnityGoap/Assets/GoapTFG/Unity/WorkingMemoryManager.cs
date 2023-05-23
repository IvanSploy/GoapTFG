using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using Unity.VisualScripting;
using UnityEngine;

namespace GoapTFG.Unity
{
    public static class WorkingMemoryManager
    {
        private static WorkingMemory<Vector3, GoapGoapEntity> WMemory = new();
        
        //Blackboard Management
        public static void Add(string name, GoapGoapEntity ge)
        {
            MemoryFact<Vector3, GoapGoapEntity> mFact = new MemoryFact<Vector3, GoapGoapEntity>
            {
                Name = name,
                Position = ge.transform.position,
                Direction = ge.transform.forward,
                Object = ge
            };
            WMemory.Add(mFact);
        }

        public static MemoryFact<Vector3, GoapGoapEntity> Get(string name)
        {
            return WMemory.Find(name);
        }
        
        public static MemoryFact<Vector3, GoapGoapEntity> Get(GoapGoapEntity ge)
        {
            return WMemory.Find(ge);
        }
        
        public static void Update(string name, GoapGoapEntity ge)
        {
            WMemory.Remove(name);
            Add(name, ge);
        }
        
        public static void Update(GoapGoapEntity ge)
        {
            WMemory.Remove(ge);
            Add(ge.Name, ge);
        }

        public static void Remove(GoapGoapEntity ge)
        {
            WMemory.Remove(ge);
        }
    }
}
