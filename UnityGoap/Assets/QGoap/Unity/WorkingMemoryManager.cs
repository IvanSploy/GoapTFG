using QGoap.Base;
using UnityEngine;
using static QGoap.Base.PropertyManager;

namespace QGoap.Unity
{
    public static class WorkingMemoryManager
    {
        private static WorkingMemory<Vector3, GoapEntity> WorkingMemory = new();
        
        //Blackboard Management
        public static void Add(string name, GoapEntity ge)
        {
            var transform = ge.transform;
            MemoryFact<Vector3, GoapEntity> mFact = new MemoryFact<Vector3, GoapEntity>
            {
                Name = name,
                Position = transform.position,
                Direction = transform.forward,
                Object = ge
            };
            WorkingMemory.Add(mFact);
        }

        public static MemoryFact<Vector3, GoapEntity> Get(string name)
        {
            return WorkingMemory.Find(name);
        }
        
        public static MemoryFact<Vector3, GoapEntity> Get(GoapEntity ge)
        {
            return WorkingMemory.Find(ge);
        }
        
        public static MemoryFact<Vector3, GoapEntity> Get(PropertyKey key)
        {
            return WorkingMemory.Find(memoryFact => memoryFact.Object.CurrentState.Has(key));
        }
        
        public static void Update(string name, GoapEntity ge)
        {
            WorkingMemory.Remove(name);
            Add(name, ge);
        }
        
        public static void Update(GoapEntity ge)
        {
            WorkingMemory.Remove(ge);
            Add(ge.Name, ge);
        }

        public static void Remove(GoapEntity ge)
        {
            WorkingMemory.Remove(ge);
        }
    }
}
