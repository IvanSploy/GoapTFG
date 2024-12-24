using UGoap.Base;
using UnityEngine;
using static UGoap.Base.PropertyManager;

namespace UGoap.Unity
{
    public static class WorkingMemoryManager
    {
        private static WorkingMemory<Vector3, UEntity> WMemory = new();
        
        //Blackboard Management
        public static void Add(string name, UEntity ge)
        {
            var transform = ge.transform;
            MemoryFact<Vector3, UEntity> mFact = new MemoryFact<Vector3, UEntity>
            {
                Name = name,
                Position = transform.position,
                Direction = transform.forward,
                Object = ge
            };
            WMemory.Add(mFact);
        }

        public static MemoryFact<Vector3, UEntity> Get(string name)
        {
            return WMemory.Find(name);
        }
        
        public static MemoryFact<Vector3, UEntity> Get(UEntity ge)
        {
            return WMemory.Find(ge);
        }
        
        public static MemoryFact<Vector3, UEntity> Get(PropertyKey key)
        {
            return WMemory.Find(memoryFact => memoryFact.Object.CurrentState.Has(key));
        }
        
        public static void Update(string name, UEntity ge)
        {
            WMemory.Remove(name);
            Add(name, ge);
        }
        
        public static void Update(UEntity ge)
        {
            WMemory.Remove(ge);
            Add(ge.Name, ge);
        }

        public static void Remove(UEntity ge)
        {
            WMemory.Remove(ge);
        }
    }
}
