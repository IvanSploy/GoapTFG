using System;
using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using UnityEngine.Serialization;
using static UGoap.Base.UGoapPropertyManager;

namespace UGoap.Unity
{
    public abstract class UGoapAction : ScriptableObject, IGoapAction
    {
        //Scriptable
        [Header("Common Data")]
        [SerializeField] private int _cost = 1;
        [FormerlySerializedAs("preconditions")] 
        [HideInInspector] public List<ConditionProperty> Preconditions = new();
        [FormerlySerializedAs("effects")] 
        [HideInInspector] public List<EffectProperty> Effects = new();
        
        //Fields
        public string Name { get; private set; }
        private readonly GoapConditions _preconditions = new();
        private readonly GoapEffects _effects = new();

        //Updating data from the scriptable object.
        private void OnValidate()
        {
            Name = name;
            _cost = Math.Max(0, _cost);
            AddIntoPropertyGroup(Preconditions, in _preconditions);
            AddIntoPropertyGroup(Effects, in _effects);
        }

        //Procedural related.
        public virtual string GetName(GoapConditions conditions, GoapEffects effects) => Name;
        protected abstract GoapConditions GetProceduralConditions(GoapSettings settings);
        protected abstract GoapEffects GetProceduralEffects(GoapSettings settings);
        
        public bool Validate(GoapState state, GoapActionInfo actionInfo, IGoapAgent agent)
        {
            UGoapAgent uAgent = agent as UGoapAgent;
            if (uAgent)
            {
                return ProceduralValidate(state, actionInfo, uAgent);
            }

            return true;
        }
        
        public void Execute(ref GoapState state, IGoapAgent agent)
        {
            UGoapAgent uAgent = agent as UGoapAgent;
            if (uAgent)
            {
                ProceduralExecute(ref state, uAgent);
            }
        }
        
        public abstract bool ProceduralValidate(GoapState state, GoapActionInfo actionInfo, UGoapAgent agent);
        public abstract void ProceduralExecute(ref GoapState state, UGoapAgent agent);
        
        //Cost related.
        public int GetCost() => _cost;        
        public virtual int GetCost(GoapConditions goal) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public GoapConditions GetPreconditions(GoapSettings settings)
        {
            return _preconditions + GetProceduralConditions(settings);
        }

        public GoapEffects GetEffects(GoapSettings settings)
        {
            return _effects + GetProceduralEffects(settings);
        }
        public HashSet<PropertyKey> GetAffectedKeys()
        {
            HashSet<PropertyKey> affectedPropertyLists = new HashSet<PropertyKey>();
            foreach (var key in _effects.GetPropertyKeys())
            {
                affectedPropertyLists.Add(key);
            }

            var proceduralEffects = GetProceduralEffects(GoapSettings.GetDefault());

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