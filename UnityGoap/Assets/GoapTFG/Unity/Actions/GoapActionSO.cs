using System;
using System.Collections.Generic;
using GoapTFG.Base;
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
        public string Name { get; }
        private readonly PropertyGroup<PropertyList, object> _preconditions;
        private readonly PropertyGroup<PropertyList, object> _effects;
        private int _cost = 1;
        private IGoapAction<PropertyList, object> _goapActionImplementation;
        public bool IsCompleted { get; } = false;

        //Creation of the scriptable object
        protected GoapActionSO()
        {
            preconditions = new();
            effects = new();
            _preconditions = new();
            _effects = new();
        }

        //Updating data from the scriptable object.
        private void OnValidate()
        {
            _cost = Math.Max(0, _cost);
            AddIntoPropertyGroup(preconditions, in _preconditions);
            AddIntoPropertyGroup(effects, in _effects);
        }

        //Procedural related.
        protected virtual bool ProceduralConditions(PropertyGroup<PropertyList, object> worldState) => true;
        protected virtual PropertyGroup<PropertyList, object> ProceduralEffects(PropertyGroup<PropertyList, object> worldState) => null;
        protected abstract void PerformedActions(PropertyGroup<PropertyList, object> worldState);
        
        //Cost related.
        public virtual int GetCost() => _cost;
        public virtual int SetCost(int cost) => _cost = cost;
        
        //Getters
        public PropertyGroup<PropertyList, object> GetPreconditions() => _preconditions;
        public PropertyGroup<PropertyList, object> GetEffects() => _effects;

        //GOAP utilities.
        public PropertyGroup<PropertyList, object> ApplyAction(PropertyGroup<PropertyList, object> worldState)
        {
            if (!CheckAction(worldState)) return null;
            return DoApplyAction(worldState);
        }

        public PropertyGroup<PropertyList, object> Execute(PropertyGroup<PropertyList, object> worldState)
        {
            worldState = ApplyAction(worldState);
            PerformedActions(worldState);
            return worldState;
        }

        //Internal methods.
        private bool CheckAction(PropertyGroup<PropertyList, object> worldState)
        {
            if (!worldState.CheckConflict(_preconditions))
            {
                return ProceduralConditions(worldState);
            }
            return false;
        }
        
        private PropertyGroup<PropertyList, object> DoApplyAction(PropertyGroup<PropertyList, object> worldState)
        {
            worldState += _effects;
            var lastWorldState = ProceduralEffects(worldState);
            if (lastWorldState != null) worldState = lastWorldState;
            return worldState;
        }
        
        //to do
        public PropertyGroup<PropertyList, object> ApplyRegressiveAction(PropertyGroup<PropertyList, object> worldState, ref GoapGoal<PropertyList, object> goapGoal, out bool reached)
        {
            if (!ProceduralConditions(worldState))
            {
                reached = false;
                return null;
            }
            
            var ws = DoApplyAction(worldState);
            var firstState = goapGoal.GetConflicts(ws);
            ws.CheckConflict(_preconditions, out var lastState);
            if(firstState == null && lastState != null) goapGoal = new GoapGoal<PropertyList, object>(goapGoal.Name, lastState, goapGoal.PriorityLevel);
            else if (firstState != null && lastState == null) goapGoal = new GoapGoal<PropertyList, object>(goapGoal.Name, firstState, goapGoal.PriorityLevel);
            else if (firstState == null)
            {
                reached = true;
                return ws;
            }
            else if (lastState.CheckConditionsConflict(firstState))
            {
                reached = false;
                return null;
            }
            else goapGoal = new GoapGoal<PropertyList, object>(goapGoal.Name, firstState + lastState, goapGoal.PriorityLevel);
            reached = false;
            return ws;
        }
        
        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}