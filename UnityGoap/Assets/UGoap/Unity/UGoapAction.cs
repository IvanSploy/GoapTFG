using System;
using System.Collections.Generic;
using UGoap.Base;
using UGoap.Learning;
using UnityEngine;
using static UGoap.Unity.UGoapPropertyManager;

using GoapState = UGoap.Base.GoapState<UGoap.Unity.UGoapPropertyManager.PropertyKey, object>;
using ConditionGroup = UGoap.Base.GoapConditions<UGoap.Unity.UGoapPropertyManager.PropertyKey, object>;
using EffectGroup = UGoap.Base.GoapEffects<UGoap.Unity.UGoapPropertyManager.PropertyKey, object>;
using GoapGoal = UGoap.Base.GoapGoal<UGoap.Unity.UGoapPropertyManager.PropertyKey, object>;
using GoapStateInfo = UGoap.Base.GoapStateInfo<UGoap.Unity.UGoapPropertyManager.PropertyKey, object>;
using IGoapAction = UGoap.Base.IGoapAction<UGoap.Unity.UGoapPropertyManager.PropertyKey, object>;

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
        private readonly ConditionGroup _preconditions = new();
        private readonly EffectGroup _effects = new();
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
        protected abstract void PerformedActions(GoapState goapState, UGoapAgent agent);
        
        //Cost related.
        public int GetCost() => _cost;        
        public virtual int GetCost(GoapState state, GoapGoal goal)
        {
            var value = GoapQLearning.ParseToStateCode(state, goal);
            var qValue = GoapQLearning.GetQValue(value, Name);
            if(Math.Abs(qValue - GoapQLearning.InitialValue) > 0.1f)
            {
                return Math.Max((int) qValue, 0);
            }

            return _cost;
        }
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

            var stateInfo = new GoapStateInfo(new GoapState(),
                new GoapGoal("", new ConditionGroup(), 1),
                new GoapState());
            
            var proceduralEffects = GetProceduralEffects(stateInfo);

            if (proceduralEffects != null)
            {
                foreach (var key in proceduralEffects.GetKeys())
                {
                    affectedPropertyLists.Add(key);
                }
            }
            return affectedPropertyLists;
        }

        //Used only by the Agent.
        public GoapState Execute(GoapStateInfo stateInfo,
            IGoapAgent<PropertyKey, object> goapAgent)
        {
            if (!CheckAction(stateInfo))
            {
                Debug.Log("Ha habido un error al realizar el plan, siento las molestias :(");
                CheckAction(stateInfo);
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
        
        private GoapState<PropertyKey, object> DoApplyAction(GoapStateInfo<PropertyKey, object> stateInfo)
        {
            var result = stateInfo.State + GetEffects(stateInfo);
            return result;
        }
        
        public (GoapState State, GoapGoal Goal) ApplyAction(GoapStateInfo stateInfo)
        {
            //Check conflicts
            if (!Validate(stateInfo)) return (null, null);
            
            var conflicts = GetPreconditions(stateInfo).GetConflict(stateInfo.State);
            
            //Apply action
            var resultState = DoApplyAction(stateInfo);
            var resultGoal = conflicts == null ? GetVictoryGoal() : new GoapGoal<PropertyKey, object>(stateInfo.Goal.Name, conflicts, stateInfo.Goal.PriorityLevel);
            return (resultState, resultGoal);
        }

        private GoapGoal<PropertyKey, object> GetVictoryGoal()
        {
            return new GoapGoal<PropertyKey, object>("Victory", new GoapConditions<PropertyKey, object>(), 1);
        }

        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}