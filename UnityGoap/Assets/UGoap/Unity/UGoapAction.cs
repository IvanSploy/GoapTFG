using System;
using System.Collections.Generic;
using GoapTFG.Base;
using Unity.VisualScripting;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;

using PropertyGroup = GoapTFG.Base.PropertyGroup<GoapTFG.UGoap.UGoapPropertyManager.PropertyKey, object>;
using GoapGoal = GoapTFG.Base.GoapGoal<GoapTFG.UGoap.UGoapPropertyManager.PropertyKey, object>;
using GoapStateInfo = GoapTFG.Base.GoapStateInfo<GoapTFG.UGoap.UGoapPropertyManager.PropertyKey, object>;
using IGoapAction = GoapTFG.Base.IGoapAction<GoapTFG.UGoap.UGoapPropertyManager.PropertyKey, object>;

namespace GoapTFG.UGoap
{
    public abstract class UGoapAction : ScriptableObject, IGoapAction
    {
        public static int actionsApplied = 0;
        
        //Scriptable 
        [HideInInspector] public List<ConditionProperty> preconditions;
        [HideInInspector] public List<EffectProperty> effects;
        [HideInInspector] public List<PropertyKey> affectedKeys;
        
        //Fields
        public string Name { get; private set; }
        private PropertyGroup _preconditions;
        private PropertyGroup _effects;
        private int _cost = 1;
        public bool IsCompleted { get; } = false;
        private HashSet<PropertyKey> _affectedKeys;

        //Creation of the scriptable object
        protected UGoapAction()
        {
            preconditions = new();
            effects = new();
            _preconditions = new();
            _effects = new();
        }

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
        protected abstract PropertyGroup GetProceduralEffects(GoapStateInfo stateInfo);
        protected abstract void PerformedActions(PropertyGroup<PropertyKey, object> proceduralEffects, UGoapAgent agent);
        
        //Cost related.
        public int GetCost() => _cost;        
        public virtual int GetCost(GoapStateInfo stateInfo) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public PropertyGroup GetPreconditions() => _preconditions;
        public PropertyGroup GetEffects() => _effects;
        public HashSet<PropertyKey> GetAffectedKeys() => _affectedKeys;
        
        private HashSet<PropertyKey> InitializeAffectedKeys(HashSet<PropertyKey> affectedPropertyKeys = null)
        {
            HashSet<PropertyKey> affectedPropertyLists = new HashSet<PropertyKey>();
            affectedPropertyLists.AddRange(_effects.GetKeys());
            if(affectedPropertyKeys != null) affectedPropertyLists.AddRange(affectedPropertyKeys);
            return affectedPropertyLists;
        }

        //GOAP utilities.
        public (PropertyGroup state, PropertyGroup proceduralEffects) ApplyAction(GoapStateInfo stateInfo)
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
        private bool CheckAction(GoapStateInfo stateInfo)
        {
            if (!stateInfo.State.CheckConflict(_preconditions))
            {
                return ProceduralConditions(stateInfo);
            }
            //Debug.Log("Accion:" + Name + " | Estado actual: " + stateInfo.WorldState + " | Precondiciones accion: " + _preconditions);
            return false;
        }
        
        private (PropertyGroup state, PropertyGroup proceduralEffects) DoApplyAction(GoapStateInfo stateInfo)
        {
            var worldState = stateInfo.State + _effects;
            var proceduralEffects = GetProceduralEffects(new GoapStateInfo
                (worldState, stateInfo.Goal));
            if (proceduralEffects != null) worldState += proceduralEffects;
            actionsApplied++;
            return (worldState, proceduralEffects);
        }
        
        public GoapStateInfo ApplyRegressiveAction(GoapStateInfo stateInfo, out bool reached)
        {
            if (!ProceduralConditions(stateInfo))
            {
                reached = false;
                return null;
            }
            
            (var worldState, var proceduralEffects) =
                DoApplyAction(stateInfo);
            var goal = stateInfo.Goal;

            //Al aplicar un filtro solo se tienen en cuenta los efectos de la acción actual,
            //es decir, efectos anteriores son ignorados aunque si que suman en conjunto si
            //el valor se actualiza.
            var filter = _effects;
            if (proceduralEffects is not null) filter += proceduralEffects;
            var remainingGoalConditions = goal.ResolveFilteredGoal(worldState, filter);
            worldState.CheckFilteredConflict(_preconditions, out var newGoalConditions, filter);
            if(remainingGoalConditions == null && newGoalConditions != null) goal = new GoapGoal(goal.Name, newGoalConditions, goal.PriorityLevel);
            else if (remainingGoalConditions != null && newGoalConditions == null) goal = new GoapGoal(goal.Name, remainingGoalConditions, goal.PriorityLevel);
            else if (remainingGoalConditions == null)
            {
                reached = true;
                return new GoapStateInfo(worldState, GetVictoryGoal(), proceduralEffects);
            }
            else
            {
                if (newGoalConditions.CheckConditionsConflict(remainingGoalConditions))
                {
                    reached = false;
                    return null;
                }
                goal = new GoapGoal(goal.Name, remainingGoalConditions + newGoalConditions, goal.PriorityLevel);
            }
            
            reached = false;
            return new GoapStateInfo(worldState, goal, proceduralEffects);
        }
        
        public (GoapStateInfo, bool) ApplyMixedAction(PropertyGroup state, GoapGoal goal)
        {
            //Check conflicts
            var conflicts = state.GetConflict(_preconditions);
            bool proceduralCheck = ProceduralConditions(new GoapStateInfo(state, goal));
            
            //Apply action
            (var resultState, var proceduralEffects) =
                DoApplyAction(new GoapStateInfo(state, goal));
            var resultGoal = conflicts == null ? GetVictoryGoal() : new GoapGoal(goal.Name, conflicts, goal.PriorityLevel);
            return (new GoapStateInfo(resultState, resultGoal, proceduralEffects), proceduralCheck);
        }

        private GoapGoal GetVictoryGoal()
        {
            return new GoapGoal("Victory", new PropertyGroup(), 1);
        }
        
        private (PropertyGroup, GoapGoal) GetVictory(PropertyGroup worldState)
        {
            return (worldState, new GoapGoal("Victory", new PropertyGroup(), 1));
        }

        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}