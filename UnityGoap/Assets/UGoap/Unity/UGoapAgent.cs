using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UGoap.Base;
using UGoap.Learning;
using UGoap.Planner;
using UGoap.Unity.ScriptableObjects;
using UnityEngine;
using static UGoap.Unity.UGoapData;
using Debug = UnityEngine.Debug;

namespace UGoap.Unity
{
    [RequireComponent(typeof(Rigidbody))]
    public class UGoapAgent : MonoBehaviour, IGoapAgent
    {
        [SerializeField] private UGoapState initialState;
        [SerializeField] private List<PriorityGoal> goalObjects;
        [SerializeField] private List<UGoapAction> actionObjects;
        [SerializeField] private GoapQLearning _goapQLearning;
        
        [SerializeField] private Rigidbody _rigidbody;
        
        public string Name { get; set; }
        
        public bool active = true;
        //public bool mixedPlan = true;
        public bool wait = true;
        public bool greedy;
        public float speed = 5;
        
        public bool hasPlan;
        public bool performingAction;

        //Agent base related
        private Stack<Node> _currentPlan;
        private List<GoapGoal> _goals;
        private List<IGoapAction> _actions;
        private GoapGoal _currentGoal;
        
        public GoapState CurrentGoapState { get; set; }

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
            List<GoapGoal> myGoals = new();
            List<IGoapAction> myActions = new();
            CurrentGoapState = initialState != null ? initialState.Create() : new();
            
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
                Debug.Log("Estado actual: " + CurrentGoapState);
                var id = CreateNewPlan(CurrentGoapState);
                if (id < 0) break;
                
                StartCoroutine(ExecutePlan());
                yield return new WaitUntil(() => !hasPlan && active);
            }

            Debug.LogWarning("No se ha encontrado plan asequible" + " | Estado actual: " +
                             CurrentGoapState);
        }

        private IEnumerator ExecutePlan()
        {
            hasPlan = true;
            GoapState result;
            do
            {
                Stopwatch stopwatch = new Stopwatch();
                result = PlanStep(CurrentGoapState);
                if (result != null){ CurrentGoapState = result;}
                yield return new WaitWhile(() => performingAction);
                stopwatch.ElapsedMilliseconds;
            } while (result != null);

            hasPlan = false;
        }

        //INTERFACE CLASSES

        public void AddAction(IGoapAction action)
        {
            _actions.Add(action);
        }

        public void AddActions(List<IGoapAction> actionList)
        {
            _actions.AddRange(actionList);
        }

        public void AddGoal(GoapGoal goal)
        {
            _goals.Add(goal);
            SortGoals();
        }

        public void AddGoals(List<GoapGoal> goalList)
        {
            _goals.AddRange(goalList);
            SortGoals();
        }

        public void SortGoals()
        {
            _goals.Sort((g1, g2) => g2.PriorityLevel.CompareTo(g1.PriorityLevel));
        }

        public int CreateNewPlan(GoapState worldGoapState)
        {
            if (_goals == null || _actions.Count == 0) return -1;
            var i = 0;
            var created = false;
            while (i < _goals.Count && !created)
            {
                _currentGoal = _goals[i];
                created = CreatePlan(worldGoapState, _currentGoal, GetCustomHeuristic());
                i++;
            }

            if (!created) return -1;
            return i - 1;
        }

        public GoapGoal GetCurrentGoal()
        {
            return _currentGoal;
        }

        public bool CreatePlan(GoapState worldGoapState, GoapGoal goapGoal,
            Func<GoapGoal, GoapState, int> customHeuristic)
        {
            var generator = new AStar(worldGoapState, customHeuristic);
            var planner = new GoapPlanner(generator);
            
            //TODO improve another learnings.
            //planner.OnNodeCreated += UpdateQValue;
            //planner.OnPlanCreated += UpdatePlanQValue;
            
            var plan = planner.CreatePlan(worldGoapState, goapGoal, _actions);
            
            DebugLogs(DebugRecord.GetRecords());
            if(_goapQLearning) _goapQLearning.DebugLearning();
            if (plan == null) return false;
            _currentPlan = plan;
            return true;
        }

        public GoapState DoPlan(GoapState worldGoapState)
        {
            if (_currentPlan.Count == 0) return null;

            foreach (var node in _currentPlan)
            {
                var stateInfo = new GoapStateInfo(worldGoapState, node.Parent.Goal, node.Parent.State);
                worldGoapState = node.ParentAction.Execute(stateInfo, this);
            }

            _currentPlan.Clear();
            return worldGoapState;
        }

        public GoapState PlanStep(GoapState worldGoapState)
        {
            if (_currentPlan.Count == 0) return null;

            Node node = _currentPlan.Pop();
            var stateInfo = new GoapStateInfo(worldGoapState, node.Parent.Goal, node.Parent.State);
            worldGoapState = node.ParentAction.Execute(stateInfo, this);
            Debug.Log(worldGoapState);
            if (worldGoapState != null)
            {
                UpdatePlanLearning(node);
            }
            return worldGoapState;
        }

        public int Count()
        {
            return _currentPlan.Count;
        }

        //ACTIONS
        public void GoToTarget(string target, float speedFactor)
        {
            StartCoroutine(Movement(speed * speedFactor, UGoapWMM.Get(target).Position));
        }
        
        public void GoGenericAction(float seconds)
        {
            if(wait) StartCoroutine(Wait(seconds));
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
        
        //Debug
        private void DebugLogs(List<string> logs)
        {
            foreach (var log in logs)
            {
                Debug.Log(log);
            }
        }
        
        //QLearning
        private void UpdatePlanLearning(Node node)
        {
            //TODO Evaluate proximity to goal.
            
            //TODO Evaluate time consumed by each action.
        }
        
        private void UpdateQValue(Node node, float reward)
        {
            if (!_goapQLearning) return;
            if (node.Parent == null) return;
            
            //Todo check if child is parent or what.
            int initialNode = _goapQLearning.ParseToStateCode(node.State);
            int finishNode = _goapQLearning.ParseToStateCode(node.Parent.State);
            _goapQLearning.UpdateQValue(initialNode, node.ParentAction.Name, reward, finishNode);
        }
        
    }
}