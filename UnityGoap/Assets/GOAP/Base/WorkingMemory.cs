using System.Collections.Generic;
using UnityEngine;

namespace GoapTFG.Base
{
    /// <summary>
    /// Represents the info recollected by the sensors.
    /// </summary>
    /// <typeparam name="TA">Vector3 class</typeparam>
    /// <typeparam name="TB">Object class</typeparam>
    public class WorkingMemory<TA, TB>
    {
        //Datos
        private List<MemoryFact<TA, TB>> _facts;
        
        //Constructor
        public WorkingMemory()
        {
            _facts = new();
        }

        //Getters and Setters
        public void Add(MemoryFact<TA, TB> fact)
        {
            _facts.Add(fact);
        }
        
        public void Remove(MemoryFact<TA, TB> fact)
        {
            _facts.Remove(fact);
        }
        
        public void Remove(string name)
        {
            _facts.RemoveAll(fact => fact.Name.Equals(name));
        }
        
        public void Remove(TB obj)
        {
            _facts.RemoveAll(fact => fact.Object.Equals(obj));
        }
        
        //Finders
        public MemoryFact<TA, TB> Find(string name)
        {
            return _facts.Find(fact => fact.Name.Equals(name));
        }
        
        public MemoryFact<TA, TB> Find(TB obj)
        {
            return _facts.Find(fact => fact.Object.Equals(obj));
        }
        
        public List<MemoryFact<TA, TB>> FindAll(string name)
        {
            return _facts.FindAll(fact => fact.Name.Equals(name));
        }
        
        public List<MemoryFact<TA, TB>> FindAll(TB obj)
        {
            return _facts.FindAll(fact => fact.Object.Equals(obj));
        }
    }
}