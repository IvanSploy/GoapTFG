using System;
using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity
{
    public abstract class UGoapAction : ScriptableObject, IGoapAction
    {
        //Scriptable 
        [Header("Common Data")]
        [SerializeField] private int _cost = 1;
        [HideInInspector] public List<ConditionProperty> preconditions = new();
        [HideInInspector] public List<EffectProperty> effects = new();
        
        //Fields
        public string Name { get; private set; }
        private readonly GoapConditions _preconditions = new();
        private readonly GoapEffects _effects = new();

        //Updating data from the scriptable object.
        private void OnValidate()
        {
            Name = name;
            _cost = Math.Max(0, _cost);
            AddIntoPropertyGroup(preconditions, in _preconditions);
            AddIntoPropertyGroup(effects, in _effects);
        }

        //Procedural related.
        protected abstract GoapConditions GetProceduralConditions(GoapConditions goal);
        protected abstract GoapEffects GetProceduralEffects(GoapConditions goal);
        public abstract bool Validate(GoapState state, IGoapAgent agent);
        public abstract void PerformedActions(GoapState state, IGoapAgent agent);
        
        //Cost related.
        public int GetCost() => _cost;        
        public virtual int GetCost(GoapConditions goal) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public GoapConditions GetPreconditions(GoapConditions goal)
        {
            return _preconditions + GetProceduralConditions(goal);
        }

        public GoapEffects GetEffects(GoapConditions goal)
        {
            return _effects + GetProceduralEffects(goal);
        }
        public HashSet<PropertyKey> GetAffectedKeys()
        {
            HashSet<PropertyKey> affectedPropertyLists = new HashSet<PropertyKey>();
            foreach (var key in _effects.GetPropertyKeys())
            {
                affectedPropertyLists.Add(key);
            }

            var proceduralEffects = GetProceduralEffects(new GoapConditions());

            if (proceduralEffects != null)
            {
                foreach (var key in proceduralEffects.GetPropertyKeys())
                {
                    affectedPropertyLists.Add(key);
                }
            }
            return affectedPropertyLists;
        }

        //Overrides
        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}