using System.Collections;
using System.Collections.Generic;

namespace GoapTFG.Base
{
    public class GoapGoal<TValue, TKey> : IEnumerable<TValue>
    {
        //Fields
        private readonly PropertyGroup<TValue, TKey> _conditions;

        //Properties
        public string Name { get; }
        public int PriorityLevel { get; }
        
        //Constructors
        public GoapGoal(string name, PropertyGroup<TValue, TKey> goal, int priorityLevel)
        {
            _conditions = new PropertyGroup<TValue, TKey>(goal) ;
            PriorityLevel = priorityLevel;
            Name = name;
        }

        //Getters
        public PropertyGroup<TValue, TKey> GetState()
        {
            return _conditions;
        }
        
        //GOAP Utilites
        public bool IsReached (PropertyGroup<TValue, TKey> worldState)
        {
            return !worldState.CheckConflict(_conditions);
        }
        
        public PropertyGroup<TValue, TKey> GetConflicts (PropertyGroup<TValue, TKey> worldState)
        {
            return worldState.CheckConflict(_conditions, out var mismatches) ? mismatches : null;
        }
        
        public PropertyGroup<TValue, TKey> GetFilteredConflicts (PropertyGroup<TValue, TKey> worldState, PropertyGroup<TValue, TKey> filter)
        {
            return worldState.CheckFilteredConflicts(_conditions, out var mismatches, filter) ? mismatches : null;
        }
        
        public int CountConflicts (PropertyGroup<TValue, TKey> worldState)
        {
            return worldState.CountConflict(_conditions);
        }

        public bool Has(TValue key)
        {
            return _conditions.Has(key);
        }
        
        //Operators
        public TKey this[TValue key]
        {
            get => GetState()[key];
            set => GetState()[key] = value;
        }
        
        public static GoapGoal<TValue, TKey> operator +(GoapGoal<TValue, TKey> a, PropertyGroup<TValue, TKey> b)
        {
            var propertyGroup = a._conditions;
            return new GoapGoal<TValue, TKey>(a.Name, propertyGroup + b, a.PriorityLevel);
        }

        //Overrides

        public override string ToString()
        {
            return "Objetivo: " + Name + "\n" + _conditions;
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            return GetState().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetState().GetEnumerator();
        }
    }
}