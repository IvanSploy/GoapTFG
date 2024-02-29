using System;
using UGoap.Base;
using UnityEngine;
using static UGoap.Unity.UGoapPropertyManager;

namespace UGoap.Unity
{
    public static class UGoapWMM
    {
        private static WorkingMemory<Vector3, UGoapEntity> WMemory = new();
        
        //Blackboard Management
        public static void Add(string name, UGoapEntity ge)
        {
            var transform = ge.transform;
            MemoryFact<Vector3, UGoapEntity> mFact = new MemoryFact<Vector3, UGoapEntity>
            {
                Name = name,
                Position = transform.position,
                Direction = transform.forward,
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
        
        public static MemoryFact<Vector3, UGoapEntity> Get(PropertyKey key)
        {
            return WMemory.Find(memoryFact => memoryFact.Object.CurrentGoapState.HasKey(key));
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
