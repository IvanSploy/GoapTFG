using System;
using System.Collections.Generic;
using GoapTFG.Base;
using Unity.VisualScripting;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap
{
    public abstract class UGoapAction : ScriptableObject, IGoapAction<PropertyKey, object>
    {
        //Scriptable 
        [HideInInspector] public List<ConditionProperty> preconditions;
        [HideInInspector] public List<EffectProperty> effects;
        [HideInInspector] public List<PropertyKey> affectedKeys;
        
        //Fields
        public string Name { get; private set; }
        private PropertyGroup<PropertyKey, object> _preconditions;
        private PropertyGroup<PropertyKey, object> _effects;
        private int _cost = 1;
        public bool IsCompleted { get; } = false;
        private PropertyGroup<PropertyKey, object> _proceduralEffects;
        private HashSet<PropertyKey> _affectedKeys;

        //Creation of the scriptable object
        protected UGoapAction()
        {
            preconditions = new();
            effects = new();
            _preconditions = new();
            _effects = new();
        }

        public IGoapAction<PropertyKey, object> Clone()
        {
            Type type = GetType();
            UGoapAction instance = (UGoapAction) CreateInstance(type);
            instance.Name = Name;
            instance._preconditions.Set(_preconditions);
            instance._effects.Set(_effects);
            instance._affectedKeys = new HashSet<PropertyKey>(_affectedKeys);
            return instance;
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
        protected abstract bool ProceduralConditions(GoapStateInfo<PropertyKey, object> stateInfo);
        protected abstract PropertyGroup<PropertyKey, object> GetProceduralEffects(GoapStateInfo<PropertyKey, object> stateInfo);
        protected abstract void PerformedActions(UGoapAgent agent);
        
        //Cost related.
        public int GetCost() => _cost;        
        public virtual int GetCost(GoapStateInfo<PropertyKey, object> stateInfo) => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public PropertyGroup<PropertyKey, object> GetPreconditions() => _preconditions;
        public PropertyGroup<PropertyKey, object> GetEffects() => _effects;
        public HashSet<PropertyKey> GetAffectedKeys() => _affectedKeys;
        
        private HashSet<PropertyKey> InitializeAffectedKeys(HashSet<PropertyKey> affectedPropertyKeys = null)
        {
            HashSet<PropertyKey> affectedPropertyLists = new HashSet<PropertyKey>();
            affectedPropertyLists.AddRange(_effects.GetKeys());
            if(affectedPropertyKeys != null) affectedPropertyLists.AddRange(affectedPropertyKeys);
            return affectedPropertyLists;
        }

        //GOAP utilities.
        public PropertyGroup<PropertyKey, object> ApplyAction(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            if (!CheckAction(stateInfo)) return null;
            return DoApplyAction(stateInfo);
        }

        //Used only by the Agent.
        public PropertyGroup<PropertyKey, object> Execute(GoapStateInfo<PropertyKey, object> stateInfo,
            IGoapAgent<PropertyKey, object> goapAgent)
        {
            if (!CheckAction(stateInfo))
            {
                Debug.Log("Ha habido un error al realizar el plan, siento las molestias :(");
                return null;
            }
            var state = stateInfo.WorldState + _effects;
            if(_proceduralEffects != null) state += _proceduralEffects;
            PerformedActions((UGoapAgent) goapAgent);
            return state;
        }

        //Internal methods.
        private bool CheckAction(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            if (!stateInfo.WorldState.CheckConflict(_preconditions))
            {
                return ProceduralConditions(stateInfo);
            }
            //Debug.Log("Accion:" + Name + " | Estado actual: " + stateInfo.WorldState + " | Precondiciones accion: " + _preconditions);
            return false;
        }
        
        private PropertyGroup<PropertyKey, object> DoApplyAction(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var worldState = stateInfo.WorldState + _effects;
            _proceduralEffects = GetProceduralEffects(new GoapStateInfo<PropertyKey, object>
                (worldState, stateInfo.CurrentGoal));
            if (_proceduralEffects != null) worldState += _proceduralEffects;
            return worldState;
        }
        
        public GoapStateInfo<PropertyKey, object> ApplyRegressiveAction(GoapStateInfo<PropertyKey, object> stateInfo,
            PropertyGroup<PropertyKey, object> initialState, out bool reached)
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
            var remainingGoalConditions = goal.ResolveGoal(worldState, filter);
            worldState.CheckFilteredConflicts(_preconditions, out var newGoalConditions, filter);
            if(remainingGoalConditions == null && newGoalConditions != null) goal = new GoapGoal<PropertyKey, object>(goal.Name, newGoalConditions, goal.PriorityLevel);
            else if (remainingGoalConditions != null && newGoalConditions == null) goal = new GoapGoal<PropertyKey, object>(goal.Name, remainingGoalConditions, goal.PriorityLevel);
            else if (remainingGoalConditions == null)
            {
                reached = true;
                return GetVictoryGoal(worldState);
            }
            else
            {
                if (newGoalConditions.CheckConditionsConflict(remainingGoalConditions))
                {
                    reached = false;
                    return null;
                }
                goal = new GoapGoal<PropertyKey, object>(goal.Name, remainingGoalConditions + newGoalConditions, goal.PriorityLevel);
            }
            
            //TODO Comprobar si esto tiene sentido
            //Si las condiciones faltantes coinciden con la del estado inicial entonces se considera objetivo.
            if (goal.GetState().CountConflict(initialState) == 0)
            {
                reached = true;
                return GetVictoryGoal(worldState);
            }
            
            reached = false;
            return new GoapStateInfo<PropertyKey, object>(worldState, goal);
        }

        private GoapStateInfo<PropertyKey, object> GetVictoryGoal(PropertyGroup<PropertyKey, object> worldState)
        {
            return new GoapStateInfo<PropertyKey, object>(worldState,
                new GoapGoal<PropertyKey, object>("Victory",
                    new PropertyGroup<PropertyKey, object>(), 1));
        }

        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}