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
        public bool IsCompleted { get; } = false;

        //Updating data from the scriptable object.
        private void OnValidate()
        {
            Name = name;
            _cost = Math.Max(0, _cost);
            AddIntoPropertyGroup(preconditions, in _preconditions);
            AddIntoPropertyGroup(effects, in _effects);
        }

        //Procedural related.
        protected abstract GoapConditions GetProceduralConditions(UGoapGoal goal);
        protected abstract GoapEffects GetProceduralEffects(UGoapGoal goal);
        protected abstract bool Validate(GoapState state);
        protected abstract bool PerformedActions(GoapState state, UGoapAgent agent);
        
        //Cost related.

        public int GetCost() => _cost;        
        public virtual int GetCost(UGoapGoal goal) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public GoapConditions GetPreconditions(UGoapGoal goal)
        {
            return _preconditions + GetProceduralConditions(goal);
        }

        public GoapEffects GetEffects(UGoapGoal goal)
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

            var goal = new UGoapGoal("", new GoapConditions(), 1);
            
            var proceduralEffects = GetProceduralEffects(goal);

            if (proceduralEffects != null)
            {
                foreach (var key in proceduralEffects.GetPropertyKeys())
                {
                    affectedPropertyLists.Add(key);
                }
            }
            return affectedPropertyLists;
        }

        //Used only by the Agent.
        public (GoapState, bool) Execute(GoapState currentState, UGoapGoal currentGoal, IGoapAgent goapAgent)
        {
            if (!CheckAction(currentState, currentGoal))
            {
                Debug.Log("Ha habido un error al realizar el plan, siento las molestias :(");
                return (null, false);
            }
            
            var state = currentState + GetEffects(currentGoal);
            var accomplished = PerformedActions(state, (UGoapAgent) goapAgent);
            
            if (!accomplished)
            {
                Debug.Log("Ha habido un error al realizar el plan, siento las molestias :(");
            }
            
            return (state, accomplished);
        }

        //Internal methods.
        private bool CheckAction(GoapState state, UGoapGoal goal)
        {
            if (!GetPreconditions(goal).CheckConflict(state))
            {
                return Validate(state);
            }
            //Debug.Log("Accion:" + Name + " | Estado actual: " + stateInfo.WorldState + " | Precondiciones accion: " + _preconditions);
            return false;
        }
        
        //TODO Actions no longer apply themselfs, they apply to goals in this case.
        private GoapState DoApplyAction(UGoapGoal goal)
        {
            var result = goal + GetEffects(goal);
            return result;
        }
        
        public UGoapGoal ApplyAction(UGoapGoal goal)
        {
            //Check conflicts.
            var conflicts = GetPreconditions(stateInfo).GetConflict(stateInfo.State);
            
            //Apply action
            var resultState = DoApplyAction(stateInfo);
            var resultGoal = conflicts == null ? GetVictoryGoal() : new UGoapGoal(stateInfo.Goal.Name, conflicts, stateInfo.Goal.PriorityLevel);
            return (resultState, resultGoal);
        }

        private UGoapGoal GetVictoryGoal()
        {
            return new UGoapGoal("Victory", new GoapConditions(), 1);
        }

        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}