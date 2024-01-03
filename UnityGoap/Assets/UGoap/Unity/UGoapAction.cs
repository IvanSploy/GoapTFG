using System;
using System.Collections.Generic;
using System.Linq;
using UGoap.Base;
using UnityEngine;
using static UGoap.Unity.UGoapPropertyManager;

using PropertyGroup = UGoap.Base.StateGroup<UGoap.Unity.UGoapPropertyManager.PropertyKey, object>;
using ConditionGroup = UGoap.Base.ConditionGroup<UGoap.Unity.UGoapPropertyManager.PropertyKey, object>;
using EffectGroup = UGoap.Base.EffectGroup<UGoap.Unity.UGoapPropertyManager.PropertyKey, object>;
using GoapGoal = UGoap.Base.GoapGoal<UGoap.Unity.UGoapPropertyManager.PropertyKey, object>;
using GoapStateInfo = UGoap.Base.GoapStateInfo<UGoap.Unity.UGoapPropertyManager.PropertyKey, object>;
using IGoapAction = UGoap.Base.IGoapAction<UGoap.Unity.UGoapPropertyManager.PropertyKey, object>;

namespace UGoap.Unity
{
    public abstract class UGoapAction : ScriptableObject, IGoapAction
    {
        //Scriptable 
        [HideInInspector] public List<ConditionProperty> preconditions = new();
        [HideInInspector] public List<EffectProperty> effects = new();
        
        //Fields
        public string Name { get; private set; }
        private readonly ConditionGroup _preconditions = new();
        private readonly EffectGroup _effects = new();
        private int _cost = 1;
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
        protected abstract bool Validate(GoapStateInfo stateInfo);
        protected abstract ConditionGroup GetProceduralConditions(GoapStateInfo stateInfo);
        protected abstract EffectGroup GetProceduralEffects(GoapStateInfo stateInfo);
        protected abstract void PerformedActions(PropertyGroup state, UGoapAgent agent);
        
        //Cost related.
        public int GetCost() => _cost;        
        public virtual int GetCost(GoapStateInfo stateInfo) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public ConditionGroup GetPreconditions(GoapStateInfo stateInfo)
        {
            return _preconditions + GetProceduralConditions(stateInfo);
        }

        public EffectGroup GetEffects(GoapStateInfo stateInfo)
        {
            return _effects + GetProceduralEffects(stateInfo);
        }
        public HashSet<PropertyKey> GetAffectedKeys()
        {
            HashSet<PropertyKey> affectedPropertyLists = new HashSet<PropertyKey>();
            foreach (var key in _effects.GetKeys())
            {
                affectedPropertyLists.Add(key);
            }

            var proceduralEffects = GetProceduralEffects(new GoapStateInfo(new PropertyGroup(),
                new GoapGoal("", new ConditionGroup(), 1)));

            if (proceduralEffects != null)
            {
                foreach (var key in proceduralEffects.GetKeys())
                {
                    affectedPropertyLists.Add(key);
                }
            }
            return affectedPropertyLists;
        }

        //GOAP utilities.
        public PropertyGroup ApplyAction(GoapStateInfo stateInfo)
        {
            if (!CheckAction(stateInfo)) return null;
            return DoApplyAction(stateInfo);
        }

        //Used only by the Agent.
        public PropertyGroup Execute(GoapStateInfo stateInfo,
            IGoapAgent<PropertyKey, object> goapAgent)
        {
            if (!CheckAction(stateInfo))
            {
                Debug.Log("Ha habido un error al realizar el plan, siento las molestias :(");
                return null;
            }
            var state = stateInfo.State + GetEffects(stateInfo);
            PerformedActions(state, (UGoapAgent) goapAgent);
            return state;
        }

        //Internal methods.
        private bool CheckAction(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            if (!GetPreconditions(stateInfo).CheckConflict(stateInfo.State))
            {
                return Validate(stateInfo);
            }
            //Debug.Log("Accion:" + Name + " | Estado actual: " + stateInfo.WorldState + " | Precondiciones accion: " + _preconditions);
            return false;
        }
        
        private StateGroup<PropertyKey, object> DoApplyAction(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            return stateInfo.State + GetEffects(stateInfo);
        }
        
        public GoapStateInfo<PropertyKey, object> ApplyRegressiveAction(GoapStateInfo<PropertyKey, object> stateInfo, out bool reached)
        {
            if (!Validate(stateInfo))
            {
                reached = false;
                return null;
            }
            
            var worldState = DoApplyAction(stateInfo);
            var goal = stateInfo.Goal;

            //Filtro de objetivos (solo los que atañen a la accion actual.)
            StateGroup<PropertyKey, object> filter = GetEffects(stateInfo);
            
            var remainingGoalConditions = goal.ResolveFilteredGoal(worldState, filter);
            GetPreconditions(stateInfo).CheckFilteredConflict(worldState, out var newGoalConditions, filter);
            
            if (remainingGoalConditions == null && newGoalConditions == null)
            {
                reached = true;
                return new GoapStateInfo<PropertyKey, object>(worldState, GetVictoryGoal());
            }

            ConditionGroup<PropertyKey, object> conditionGroup = ConditionGroup<PropertyKey, object>.Merge(remainingGoalConditions, newGoalConditions);
            if (conditionGroup == null)
            {
                reached = false;
                return null;
            }
            
            goal = new GoapGoal<PropertyKey, object>(goal.Name, conditionGroup, goal.PriorityLevel);

            reached = false;
            return new GoapStateInfo<PropertyKey, object>(worldState, goal);
        }
        
        public GoapStateInfo<PropertyKey, object> ApplyMixedAction(StateGroup<PropertyKey, object> state, GoapGoal<PropertyKey, object> goal)
        {
            //Check conflicts
            var stateInfo = new GoapStateInfo<PropertyKey, object>(state, goal);
            var conflicts = GetPreconditions(stateInfo).GetConflict(state);
            bool proceduralCheck = Validate(stateInfo);
            
            //Apply action
            var resultState = DoApplyAction(stateInfo);
            var resultGoal = conflicts == null ? GetVictoryGoal() : new GoapGoal<PropertyKey, object>(goal.Name, conflicts, goal.PriorityLevel);
            return (new GoapStateInfo<PropertyKey, object>(resultState, resultGoal), proceduralCheck);
        }

        private GoapGoal<PropertyKey, object> GetVictoryGoal()
        {
            return new GoapGoal<PropertyKey, object>("Victory", new ConditionGroup<PropertyKey, object>(), 1);
        }

        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}