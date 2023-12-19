using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GoapTFG.Base
{
    public class GoapGoal<TKey, TValue> : IEnumerable<TKey>
    {
        //Fields
        private readonly PropertyGroup<TKey, TValue> _conditions;

        //Properties
        public string Name { get; }
        public int PriorityLevel { get; }
        
        //Constructors
        public GoapGoal(string name, PropertyGroup<TKey, TValue> goal, int priorityLevel)
        {
            _conditions = new PropertyGroup<TKey, TValue>(goal) ;
            PriorityLevel = priorityLevel;
            Name = name;
        }
        
        public GoapGoal(GoapGoal<TKey, TValue> goapGoal)
        {
            _conditions = new PropertyGroup<TKey, TValue>(goapGoal._conditions) ;
            PriorityLevel = goapGoal.PriorityLevel;
            Name = goapGoal.Name;
        }

        //Getters
        public PropertyGroup<TKey, TValue> GetState()
        {
            return _conditions;
        }
        
        //GOAP Utilites
        public bool IsEmpty()
        {
            return _conditions.IsEmpty();
        }
        
        public bool IsReached (PropertyGroup<TKey, TValue> worldState)
        {
            return !worldState.CheckConflict(_conditions);
        }
        
        public PropertyGroup<TKey, TValue> GetConflicts (PropertyGroup<TKey, TValue> worldState)
        {
            return worldState.GetConflict(_conditions);
        }
        
        public PropertyGroup<TKey, TValue> ResolveFilteredGoal (PropertyGroup<TKey, TValue> worldState, PropertyGroup<TKey, TValue> filter)
        {
            return worldState.CheckFilteredConflict(_conditions, out var mismatches, filter) ? mismatches : null;
        }
        
        public int CountConflicts (PropertyGroup<TKey, TValue> worldState)
        {
            return worldState.CountConflict(_conditions);
        }

        public bool Has(TKey key)
        {
            return _conditions.HasKey(key);
        }
        
        //Operators
        public TValue this[TKey key]
        {
            get => GetState()[key];
            set => GetState()[key] = value;
        }
        
        public static GoapGoal<TKey, TValue> operator +(GoapGoal<TKey, TValue> a, PropertyGroup<TKey, TValue> b)
        {
            var propertyGroup = a._conditions;
            return new GoapGoal<TKey, TValue>(a.Name, propertyGroup + b, a.PriorityLevel);
        }

        //Overrides

        public override string ToString()
        {
            return "Objetivo: " + Name + "\n" + _conditions;
        }

        public IEnumerator<TKey> GetEnumerator()
        {
            return GetState().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetState().GetEnumerator();
        }
    }
}