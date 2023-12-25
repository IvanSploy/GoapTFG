using System.Collections.Generic;

namespace UGoap.Base
{
    /// <summary>
    /// Represents the info recollected by the sensors.
    /// </summary>
    /// <typeparam name="TVector">Vector3 class</typeparam>
    /// <typeparam name="TObject">Object class</typeparam>
    public class WorkingMemory<TVector, TObject>
    {
        //Datos
        private List<MemoryFact<TVector, TObject>> _facts;
        
        //Constructor
        public WorkingMemory()  
        {
            _facts = new();
        }

        //Getters and Setters
        public void Add(MemoryFact<TVector, TObject> fact)
        {
            _facts.Add(fact);
        }
        
        public void Remove(MemoryFact<TVector, TObject> fact)
        {
            _facts.Remove(fact);
        }
        
        public void Remove(string name)
        {
            _facts.RemoveAll(fact => fact.Name.Equals(name));
        }
        
        public void Remove(TObject obj)
        {
            _facts.RemoveAll(fact => fact.Object.Equals(obj));
        }
        
        //Finders
        public MemoryFact<TVector, TObject> Find(string name)
        {
            return _facts.Find(fact => fact.Name.Equals(name));
        }
        
        public MemoryFact<TVector, TObject> Find(TObject obj)
        {
            return _facts.Find(fact => fact.Object.Equals(obj));
        }
        
        public List<MemoryFact<TVector, TObject>> FindAll(string name)
        {
            return _facts.FindAll(fact => fact.Name.Equals(name));
        }
        
        public List<MemoryFact<TVector, TObject>> FindAll(TObject obj)
        {
            return _facts.FindAll(fact => fact.Object.Equals(obj));
        }
    }
}