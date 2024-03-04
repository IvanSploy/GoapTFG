using System;
using System.Collections.Generic;
using UGoap.Base;
using UGoap.Learning;
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
        protected abstract bool Validate(GoapStateInfo stateInfo);
        protected abstract GoapConditions GetProceduralConditions(GoapStateInfo stateInfo);
        protected abstract GoapEffects GetProceduralEffects(GoapStateInfo stateInfo);
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
        public GoapConditions GetPreconditions(GoapStateInfo stateInfo)
        {
            return _preconditions + GetProceduralConditions(stateInfo);
        }

        public GoapEffects GetEffects(GoapStateInfo stateInfo)
        {
            return _effects + GetProceduralEffects(stateInfo);
        }
        public HashSet<PropertyKey> GetAffectedKeys()
        {
            HashSet<PropertyKey> affectedPropertyLists = new HashSet<PropertyKey>();
            foreach (var key in _effects.GetPropertyKeys())
            {
                affectedPropertyLists.Add(key);
            }

            var stateInfo = new GoapStateInfo(new GoapState(),
                new GoapGoal("", new GoapConditions(), 1),
                new GoapState());
            
            var proceduralEffects = GetProceduralEffects(stateInfo);

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
        public GoapState Execute(GoapStateInfo stateInfo,
            IGoapAgent goapAgent)
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
        private bool CheckAction(GoapStateInfo stateInfo)
        {
            if (!GetPreconditions(stateInfo).CheckConflict(stateInfo.State))
            {
                return Validate(stateInfo);
            }
            //Debug.Log("Accion:" + Name + " | Estado actual: " + stateInfo.WorldState + " | Precondiciones accion: " + _preconditions);
            return false;
        }
        
        private GoapState DoApplyAction(GoapStateInfo stateInfo)
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
            var resultGoal = conflicts == null ? GetVictoryGoal() : new GoapGoal(stateInfo.Goal.Name, conflicts, stateInfo.Goal.PriorityLevel);
            return (resultState, resultGoal);
        }

        private GoapGoal GetVictoryGoal()
        {
            return new GoapGoal("Victory", new GoapConditions(), 1);
        }

        public override string ToString()
        {
            return Name 
                   + " ->\nPreconditions:\n" + _preconditions + "Effects:\n" + _effects
                   ;
        }
    }
}