using System;
using System.Collections.Generic;
using GoapTFG.Base;
using Unity.VisualScripting;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity
{
    public abstract class GoapActionSO : ScriptableObject, IGoapAction<PropertyList, object>
    {
        //Scriptable Object
        public static bool GenerateActionNames;
        [HideInInspector] public List<ConditionProperty> preconditions;
        [HideInInspector] public List<EffectProperty> effects;
        
        //Fields
        public string Name { get; private set; }
        private PropertyGroup<PropertyList, object> _preconditions;
        private PropertyGroup<PropertyList, object> _effects;
        private PropertyGroup<PropertyList, object> _proceduralEffects;
        private int _cost = 1;
        public bool IsCompleted { get; } = false;

        //Creation of the scriptable object
        protected GoapActionSO()
        {
            preconditions = new();
            effects = new();
            _preconditions = new();
            _effects = new();
        }

        public GoapActionSO Clone()
        {
            Type type = GetType();
            GoapActionSO instance = (GoapActionSO) CreateInstance(type);
            instance.Name = Name;
            instance._preconditions += _preconditions;
            instance._effects += _effects;
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
        protected abstract bool ProceduralConditions(GoapStateInfo<PropertyList, object> stateInfo);
        protected abstract PropertyGroup<PropertyList, object> GetProceduralEffects(GoapStateInfo<PropertyList, object> stateInfo);
        protected abstract void PerformedActions(GoapAgent goapAgent);
        
        //Cost related.
        public virtual int GetCost() => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public PropertyGroup<PropertyList, object> GetPreconditions() => _preconditions;
        public PropertyGroup<PropertyList, object> GetEffects() => _effects;

        //GOAP utilities.
        public PropertyGroup<PropertyList, object> ApplyAction(GoapStateInfo<PropertyList, object> stateInfo)
        {
            if (!CheckAction(stateInfo)) return null;
            return DoApplyAction(stateInfo);
        }

        //Used only by the Agent.
        public PropertyGroup<PropertyList, object> Execute(PropertyGroup<PropertyList, object> worldState,
            IGoapAgent<PropertyList, object> goapAgent)
        {
            worldState += _effects;
            if(_proceduralEffects != null) worldState += _proceduralEffects;
            PerformedActions((GoapAgent) goapAgent);
            return worldState;
        }

        //Internal methods.
        private bool CheckAction(GoapStateInfo<PropertyList, object> stateInfo)
        {
            if (!stateInfo.WorldState.CheckConflict(_preconditions))
            {
                return ProceduralConditions(stateInfo);
            }
            return false;
        }
        
        private PropertyGroup<PropertyList, object> DoApplyAction(GoapStateInfo<PropertyList, object> stateInfo)
        {
            var worldState = stateInfo.WorldState + _effects;
            _proceduralEffects = GetProceduralEffects(new GoapStateInfo<PropertyList, object>
                (worldState, stateInfo.CurrentGoal));
            if (_proceduralEffects != null) worldState += _proceduralEffects;
            return worldState;
        }
        
        public GoapStateInfo<PropertyList, object> ApplyRegressiveAction(GoapStateInfo<PropertyList, object> stateInfo, out bool reached)
        {
            if (!ProceduralConditions(stateInfo))
            {
                reached = false;
                return null;
            }
            
            var worldState = DoApplyAction(stateInfo);
            var goapGoal = stateInfo.CurrentGoal;
            
            var firstState = goapGoal.GetConflicts(worldState);
            worldState.CheckConflict(_preconditions, out var lastState);
            
            if(firstState == null && lastState != null) goapGoal = new GoapGoal<PropertyList, object>(goapGoal.Name, lastState, goapGoal.PriorityLevel);
            else if (firstState != null && lastState == null) goapGoal = new GoapGoal<PropertyList, object>(goapGoal.Name, firstState, goapGoal.PriorityLevel);
            else if (firstState == null)
            {
                reached = true;
                return new GoapStateInfo<PropertyList, object>(worldState,
                    new GoapGoal<PropertyList, object>("Victory",
                        new PropertyGroup<PropertyList, object>(), 1));
            }
            else if (lastState.CheckConditionsConflict(firstState))
            {
                reached = false;
                return null;
            }
            else goapGoal = new GoapGoal<PropertyList, object>(goapGoal.Name, firstState + lastState, goapGoal.PriorityLevel);
            reached = false;
            return new GoapStateInfo<PropertyList, object>(worldState, goapGoal);
        }

        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}