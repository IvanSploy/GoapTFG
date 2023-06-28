using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using Unity.VisualScripting;
using UnityEngine;

namespace GoapTFG.UGoap
{
    public static class UGoapWMM
    {
        private static WorkingMemory<Vector3, UGoapEntity> WMemory = new();
        
        //Blackboard Management
        public static void Add(string name, UGoapEntity ge)
        {
            MemoryFact<Vector3, UGoapEntity> mFact = new MemoryFact<Vector3, UGoapEntity>
            {
                Name = name,
                Position = ge.transform.position,
                Direction = ge.transform.forward,
                Object = ge
            };
            WMemory.Add(mFact);
        }

        public static MemoryFact<Vector3, UGoapEntity> Get(string name)
        {
            return WMemory.Find(name);
        }
        
        public static MemoryFact<Vector3, UGoapEntity> Get(UGoapEntity ge)
        {
            return WMemory.Find(ge);
        }
        
        public static void Update(string name, UGoapEntity ge)
        {
            WMemory.Remove(name);
            Add(name, ge);
        }
        
        public static void Update(UGoapEntity ge)
        {
            WMemory.Remove(ge);
            Add(ge.Name, ge);
        }

        public static void Remove(UGoapEntity ge)
        {
            WMemory.Remove(ge);
        }
    }
}
