using System;
using System.Collections.Generic;
using System.Linq;
using UGoap.Base;
using UnityEngine;
using static UGoap.Unity.UGoapPropertyManager;

using PropertyGroup = UGoap.Base.PropertyGroup<UGoap.Unity.UGoapPropertyManager.PropertyKey, object>;
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
        [HideInInspector] public List<PropertyKey> affectedKeys;
        
        //Fields
        public string Name { get; private set; }
        private ConditionGroup _preconditions = new();
        private EffectGroup _effects = new();
        private int _cost = 1;
        public bool IsCompleted { get; } = false;
        private HashSet<PropertyKey> _affectedKeys;

        //Updating data from the scriptable object.
        private void OnValidate()
        {
            Name = name;
            _cost = Math.Max(0, _cost);
            AddIntoPropertyGroup(preconditions, in _preconditions);
            AddIntoPropertyGroup(effects, in _effects);
            _affectedKeys = InitializeAffectedKeys(affectedKeys.ToHashSet());
        }

        //Procedural related.
        protected abstract bool ProceduralConditions(GoapStateInfo stateInfo);
        protected abstract EffectGroup GetProceduralEffects(GoapStateInfo stateInfo);
        protected abstract void PerformedActions(EffectGroup proceduralEffects, UGoapAgent agent);
        
        //Cost related.
        public int GetCost() => _cost;        
        public virtual int GetCost(GoapStateInfo stateInfo) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public ConditionGroup GetPreconditions() => _preconditions;
        public EffectGroup GetEffects() => _effects;
        public HashSet<PropertyKey> GetAffectedKeys() => _affectedKeys;
        
        private HashSet<PropertyKey> InitializeAffectedKeys(HashSet<PropertyKey> affectedPropertyKeys = null)
        {
            HashSet<PropertyKey> affectedPropertyLists = new HashSet<PropertyKey>();
            foreach (var key in _effects.GetKeys())
            {
                affectedPropertyLists.Add(key);
            }

            if (affectedKeys != null)
            {
                foreach (var key in affectedKeys)
                {
                    affectedPropertyLists.Add(key);
                }
            }
            return affectedPropertyLists;
        }

        //GOAP utilities.
        public (PropertyGroup state, EffectGroup proceduralEffects) ApplyAction(GoapStateInfo stateInfo)
        {
            if (!CheckAction(stateInfo)) return (null, null);
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
            var state = stateInfo.State + _effects;
            if(stateInfo.ProceduralEffects != null) state += stateInfo.ProceduralEffects;
            PerformedActions(stateInfo.ProceduralEffects, (UGoapAgent) goapAgent);
            return state;
        }

        //Internal methods.
        private bool CheckAction(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            if (!_preconditions.CheckConflict(stateInfo.State))
            {
                return ProceduralConditions(stateInfo);
            }
            //Debug.Log("Accion:" + Name + " | Estado actual: " + stateInfo.WorldState + " | Precondiciones accion: " + _preconditions);
            return false;
        }
        
        private (PropertyGroup<PropertyKey, object> state, EffectGroup<PropertyKey, object> proceduralEffects) DoApplyAction(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var worldState = stateInfo.State + _effects;
            var proceduralEffects = GetProceduralEffects(new GoapStateInfo<PropertyKey, object>(worldState, stateInfo.Goal));
            if (proceduralEffects != null) worldState += proceduralEffects;
            return (worldState, proceduralEffects);
        }
        
        public GoapStateInfo<PropertyKey, object> ApplyRegressiveAction(GoapStateInfo<PropertyKey, object> stateInfo, out bool reached)
        {
            if (!ProceduralConditions(stateInfo))
            {
                reached = false;
                return null;
            }
            
            (var worldState, var proceduralEffects) =
                DoApplyAction(stateInfo);
            var goal = stateInfo.Goal;

            //Filtro de objetivos (solo los que atañen a la accion actual.)
            PropertyGroup<PropertyKey, object> filter = _effects;
            if (proceduralEffects is not null) filter += proceduralEffects;
            
            var remainingGoalConditions = goal.ResolveFilteredGoal(worldState, filter);
            _preconditions.CheckFilteredConflict(worldState, out var newGoalConditions, filter);
            
            if (remainingGoalConditions == null && newGoalConditions == null)
            {
                reached = true;
                return new GoapStateInfo<PropertyKey, object>(worldState, GetVictoryGoal(), proceduralEffects);
            }

            ConditionGroup<PropertyKey, object> conditionGroup = ConditionGroup<PropertyKey, object>.Merge(remainingGoalConditions, newGoalConditions);
            if (conditionGroup == null)
            {
                reached = false;
                return null;
            }
            
            goal = new GoapGoal<PropertyKey, object>(goal.Name, conditionGroup, goal.PriorityLevel);

            reached = false;
            return new GoapStateInfo<PropertyKey, object>(worldState, goal, proceduralEffects);
        }
        
        public (GoapStateInfo<PropertyKey, object>, bool) ApplyMixedAction(PropertyGroup<PropertyKey, object> state, GoapGoal<PropertyKey, object> goal)
        {
            //Check conflicts
            var conflicts = _preconditions.GetConflict(state);
            bool proceduralCheck = ProceduralConditions(new GoapStateInfo<PropertyKey, object>(state, goal));
            
            //Apply action
            (var resultState, var proceduralEffects) =
                DoApplyAction(new GoapStateInfo<PropertyKey, object>(state, goal));
            var resultGoal = conflicts == null ? GetVictoryGoal() : new GoapGoal<PropertyKey, object>(goal.Name, conflicts, goal.PriorityLevel);
            return (new GoapStateInfo<PropertyKey, object>(resultState, resultGoal, proceduralEffects), proceduralCheck);
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