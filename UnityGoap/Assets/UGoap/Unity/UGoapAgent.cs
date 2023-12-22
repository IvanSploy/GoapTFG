using System;
using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Planner;
using GoapTFG.UGoap.ScriptableObjects;
using UnityEngine;
using static GoapTFG.UGoap.UGoapData;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap
{
    [RequireComponent(typeof(Rigidbody))]
    public class UGoapAgent : MonoBehaviour, IGoapAgent<PropertyKey, object>
    {
        [SerializeField] private UGoapState initialState;
        [SerializeField] private List<PriorityGoal> goalObjects;
        [SerializeField] private List<UGoapAction> actionObjects;
        [SerializeField] private Rigidbody _rigidbody;
        
        public string Name { get; set; }
        
        public bool active = true;
        public bool hasPlan;
        public bool performingAction = false;
        public bool mixedPlan = true;
        public bool greedy = false;
        public float speed = 5;

        //Agent base related
        private Stack<GoapActionData<PropertyKey, object>> _currentPlan;
        private List<GoapGoal<PropertyKey, object>> _goals;
        private List<IGoapAction<PropertyKey, object>> _actions;
        private GoapGoal<PropertyKey, object> _currentGoal;
        
        public PropertyGroup<PropertyKey, object> CurrentState { get; set; }

        // Start is called before the first frame update

        private void Awake()
        {
            _rigidbody ??= GetComponent<Rigidbody>(); 
            gameObject.layer = LayerMask.NameToLayer("Agent");
        }

        void Start()
        {
            _currentPlan = new();
            _goals = new();
            _actions = new();
            List<GoapGoal<PropertyKey, object>> myGoals = new();
            List<IGoapAction<PropertyKey, object>> myActions = new();
            CurrentState = initialState != null ? initialState.Create() : new();
            
            //OBJETIVOS
            foreach (var goal in goalObjects)
            {
                _goals.Add(goal.Create());
            }

            //ACCIONES
            foreach (var action in actionObjects)
            {
                _actions.Add(action);
            }

            SortGoals();

            //Se crea el blackboard utilizado por las acciones de GOAP.
            StartCoroutine(Replan());
        }

        //CORRUTINAS
        private IEnumerator Replan()
        {
            while (true)
            {
                Debug.Log("Estado actual: " + CurrentState);
                var id = CreateNewPlan(CurrentState);
                if (id < 0) break;
                
                StartCoroutine(ExecutePlan());
                yield return new WaitUntil(() => !hasPlan && active);
            }

            Debug.LogWarning("No se ha encontrado plan asequible" + " | Estado actual: " +
                             CurrentState);
        }

        private IEnumerator ExecutePlan()
        {
            hasPlan = true;
            PropertyGroup<PropertyKey, object> result;
            do
            {
                result = PlanStep(CurrentState);
                if (result != null){ CurrentState = result;}
                yield return new WaitWhile(() => performingAction);
            } while (result != null);

            hasPlan = false;
        }

        //INTERFACE CLASSES

        public void AddAction(IGoapAction<PropertyKey, object> action)
        {
            _actions.Add(action);
        }

        public void AddActions(List<IGoapAction<PropertyKey, object>> actionList)
        {
            _actions.AddRange(actionList);
        }

        public void AddGoal(GoapGoal<PropertyKey, object> goal)
        {
            _goals.Add(goal);
            SortGoals();
        }

        public void AddGoals(List<GoapGoal<PropertyKey, object>> goalList)
        {
            _goals.AddRange(goalList);
            SortGoals();
        }

        public void SortGoals()
        {
            _goals.Sort((g1, g2) => g2.PriorityLevel.CompareTo(g1.PriorityLevel));
        }

        public int CreateNewPlan(PropertyGroup<PropertyKey, object> worldState)
        {
            if (_goals == null || _actions.Count == 0) return -1;
            var i = 0;
            var created = false;
            while (i < _goals.Count && !created)
            {
                _currentGoal = _goals[i];
                created = CreatePlan(worldState, _currentGoal, GetCustomHeuristic());
                i++;
            }

            if (!created) return -1;
            return i - 1;
        }

        public GoapGoal<PropertyKey, object> GetCurrentGoal()
        {
            return _currentGoal;
        }

        public bool CreatePlan(PropertyGroup<PropertyKey, object> worldState, GoapGoal<PropertyKey, object> goapGoal,
            Func<GoapGoal<PropertyKey, object>, PropertyGroup<PropertyKey, object>, int> customHeuristic)
        {
            var plan = mixedPlan
                ? MixedPlanner<PropertyKey, object>.CreatePlan(worldState, goapGoal, _actions, customHeuristic, greedy)
                : RegressivePlanner<PropertyKey, object>.CreatePlan(worldState, goapGoal, _actions, customHeuristic);
            if (plan == null) return false;
            _currentPlan = plan;
            return true;
        }

        public PropertyGroup<PropertyKey, object> DoPlan(PropertyGroup<PropertyKey, object> worldState)
        {
            if (_currentPlan.Count == 0) return null;

            foreach (var actionData in _currentPlan)
            {
                var stateInfo = new GoapStateInfo<PropertyKey, object>(worldState, actionData.Goal, actionData.ProceduralEffects);
                worldState = actionData.Action.Execute(stateInfo, this);
            }

            _currentPlan.Clear();
            return worldState;
        }

        public PropertyGroup<PropertyKey, object> PlanStep(PropertyGroup<PropertyKey, object> worldState)
        {
            if (_currentPlan.Count == 0) return null;

            GoapActionData<PropertyKey, object> actionData = _currentPlan.Pop();
            var stateInfo = new GoapStateInfo<PropertyKey, object>(worldState, actionData.Goal, actionData.ProceduralEffects);
            worldState = actionData.Action.Execute(stateInfo, this);
            Debug.Log(worldState);
            return worldState;
        }

        public int Count()
        {
            return _currentPlan.Count;
        }

        //ACTIONS
        public void GoToTargetWalking(string target)
        {
            StartCoroutine(Movement(speed * 0.5f, UGoapWMM.Get(target).Position));
        }

        public void GoToTargetRunning(string target)
        {
            StartCoroutine(Movement(speed, UGoapWMM.Get(target).Position));
        }
        
        public void GoGenericAction(float seconds)
        {
            StartCoroutine(Wait(seconds));
        }
        
        //COROUTINES
        private IEnumerator Movement(float vel, Vector3 finalPos)
        {
            performingAction = true;
            bool reached = false;
            while (!reached)
            {
                var position = transform.position;
                finalPos.y = position.y;
                position = Vector3.MoveTowards(position, finalPos,
                    Time.deltaTime * vel);
                transform.position = position;
                Vector3 aux = finalPos;
                aux.y = position.y;
                if (Vector3.Distance(transform.position, aux) < Single.Epsilon) reached = true;
                yield return null;
            }

            performingAction = false;
        }

        
        IEnumerator Wait(float seconds)
        {
            performingAction = true;
            yield return new WaitForSeconds(seconds);
            performingAction = false;
        }
    }
}