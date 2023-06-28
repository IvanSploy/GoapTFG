using System;
using System.Collections.Generic;
using GoapTFG.Base;
using Unity.VisualScripting;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap
{
    public abstract class UGoapActionBase : ScriptableObject, IGoapAction<UGoapPropertyManager.PropertyList, object>
    {
        //Scriptable Object
        [HideInInspector] public List<UGoapPropertyManager.ConditionProperty> preconditions;
        [HideInInspector] public List<UGoapPropertyManager.EffectProperty> effects;
        
        //Fields
        public string Name { get; private set; }
        private PropertyGroup<UGoapPropertyManager.PropertyList, object> _preconditions;
        private PropertyGroup<UGoapPropertyManager.PropertyList, object> _effects;
        private PropertyGroup<UGoapPropertyManager.PropertyList, object> _proceduralEffects;
        private int _cost = 1;
        public bool IsCompleted { get; } = false;

        //Creation of the scriptable object
        protected UGoapActionBase()
        {
            preconditions = new();
            effects = new();
            _preconditions = new();
            _effects = new();
        }

        public IGoapAction<UGoapPropertyManager.PropertyList, object> Clone()
        {
            Type type = GetType();
            UGoapActionBase instance = (UGoapActionBase) CreateInstance(type);
            instance.Name = Name;
            instance._preconditions.Set(_preconditions);
            instance._effects.Set(_effects);
            return instance;
        }

        //Updating data from the scriptable object.
        private void OnValidate()
        {
            Name = name;
            _cost = Math.Max(0, _cost);
            AddIntoPropertyGroup(preconditions, in _preconditions);
            AddIntoPropertyGroup(effects, in _effects);
        }

        //Procedural related.
        protected abstract bool ProceduralConditions(GoapStateInfo<UGoapPropertyManager.PropertyList, object> stateInfo);
        protected abstract PropertyGroup<UGoapPropertyManager.PropertyList, object> GetProceduralEffects(GoapStateInfo<UGoapPropertyManager.PropertyList, object> stateInfo);
        protected abstract HashSet<UGoapPropertyManager.PropertyList> GetAffectedPropertyLists();
        protected abstract void PerformedActions(UGoapAgent agent);
        
        //Cost related.
        public int GetCost() => _cost;        
        public virtual int GetCost(GoapStateInfo<UGoapPropertyManager.PropertyList, object> stateInfo) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public PropertyGroup<UGoapPropertyManager.PropertyList, object> GetPreconditions() => _preconditions;
        public PropertyGroup<UGoapPropertyManager.PropertyList, object> GetEffects() => _effects;
        public HashSet<UGoapPropertyManager.PropertyList> GetAffectedKeys()
        {
            HashSet<UGoapPropertyManager.PropertyList> affectedPropertyLists = new HashSet<UGoapPropertyManager.PropertyList>();
            affectedPropertyLists.AddRange(_effects.GetKeys());
            var procedural = GetAffectedPropertyLists();
            procedural ??= new HashSet<UGoapPropertyManager.PropertyList>();
            affectedPropertyLists.AddRange(procedural);
            return affectedPropertyLists;
        }

        //GOAP utilities.
        public PropertyGroup<UGoapPropertyManager.PropertyList, object> ApplyAction(GoapStateInfo<UGoapPropertyManager.PropertyList, object> stateInfo)
        {
            if (!CheckAction(stateInfo)) return null;
            return DoApplyAction(stateInfo);
        }

        //Used only by the Agent.
        public PropertyGroup<UGoapPropertyManager.PropertyList, object> Execute(PropertyGroup<UGoapPropertyManager.PropertyList, object> worldState,
            IGoapAgent<UGoapPropertyManager.PropertyList, object> goapAgent)
        {
            worldState += _effects;
            if(_proceduralEffects != null) worldState += _proceduralEffects;
            PerformedActions((UGoapAgent) goapAgent);
            return worldState;
        }

        //Internal methods.
        private bool CheckAction(GoapStateInfo<UGoapPropertyManager.PropertyList, object> stateInfo)
        {
            if (!stateInfo.WorldState.CheckConflict(_preconditions))
            {
                return ProceduralConditions(stateInfo);
            }
            return false;
        }
        
        private PropertyGroup<UGoapPropertyManager.PropertyList, object> DoApplyAction(GoapStateInfo<UGoapPropertyManager.PropertyList, object> stateInfo)
        {
            var worldState = stateInfo.WorldState + _effects;
            _proceduralEffects = GetProceduralEffects(new GoapStateInfo<UGoapPropertyManager.PropertyList, object>
                (worldState, stateInfo.CurrentGoal));
            if (_proceduralEffects != null) worldState += _proceduralEffects;
            return worldState;
        }
        
        public GoapStateInfo<UGoapPropertyManager.PropertyList, object> ApplyRegressiveAction(GoapStateInfo<UGoapPropertyManager.PropertyList, object> stateInfo, out bool reached)
        {
            if (!ProceduralConditions(stateInfo))
            {
                reached = false;
                return null;
            }
            
            var worldState = DoApplyAction(stateInfo);
            var goal = stateInfo.CurrentGoal;

            //Al aplicar un filtro solo se tienen en cuenta los efectos de la acción actual,
            //es decir, efectos anteriores son ignorados aunque si que suman en conjunto si
            //el valor se actualiza.
            var filter = _effects;
            if (_proceduralEffects is not null) filter += _proceduralEffects;
            var remainingGoalConditions = goal.GetFilteredConflicts(worldState, filter);
            worldState.CheckFilteredConflicts(_preconditions, out var newGoalConditions, filter);

            if(remainingGoalConditions == null && newGoalConditions != null) goal = new GoapGoal<UGoapPropertyManager.PropertyList, object>(goal.Name, newGoalConditions, goal.PriorityLevel);
            else if (remainingGoalConditions != null && newGoalConditions == null) goal = new GoapGoal<UGoapPropertyManager.PropertyList, object>(goal.Name, remainingGoalConditions, goal.PriorityLevel);
            else if (remainingGoalConditions == null)
            {
                reached = true;
                return new GoapStateInfo<UGoapPropertyManager.PropertyList, object>(worldState,
                    new GoapGoal<UGoapPropertyManager.PropertyList, object>("Victory",
                        new PropertyGroup<UGoapPropertyManager.PropertyList, object>(), 1));
            }
            else if (newGoalConditions.CheckConditionsConflict(remainingGoalConditions))
            {
                reached = false;
                return null;
            }
            else goal = new GoapGoal<UGoapPropertyManager.PropertyList, object>(goal.Name, remainingGoalConditions + newGoalConditions, goal.PriorityLevel);
            reached = false;
            return new GoapStateInfo<UGoapPropertyManager.PropertyList, object>(worldState, goal);
        }

        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}