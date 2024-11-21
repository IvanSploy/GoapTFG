using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UGoap.Base;
using UGoap.Learning;
using UGoap.Planner;
using UGoap.Unity.ScriptableObjects;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UGoap.Unity
{
    [RequireComponent(typeof(Rigidbody))]
    public class UGoapAgent : MonoBehaviour, IGoapAgent
    {
        [SerializeField] private Rigidbody _rigidbody;
        
        [Header("Planner")]
        [SerializeField] private bool _runOnStart;
        [SerializeField] private bool _active = true;
        private float _remainingSeconds;
        [SerializeField] private float _rePlanSeconds = 5;
        [SerializeField] private StateConfig _initialStateConfig;
        [SerializeField] private List<PriorityGoal> _goalList;
        [SerializeField] private List<ActionConfig> _actionList;
        [SerializeField] private LearningConfig _learningConfig;
        
        //Agent base related
        private bool _hasPlan;
        private Plan _currentPlan;
        private readonly List<IGoapGoal> _goals = new();
        private readonly List<GoapAction> _actions = new();
        private IGoapGoal _currentGoal;
        private Coroutine _currentActionRoutine;
        
        [Header("Movement")]
        public float Speed = 5;
        
        //Agent Properties
        public string Name { get; set; }
        public bool PerformingAction { get; set; }
        public bool Interrupted { get; set; }
        public GoapState CurrentState { get; set; }

        // Start is called before the first frame update
        private void Awake()
        {
            if(!_rigidbody) _rigidbody = GetComponent<Rigidbody>();
            gameObject.layer = LayerMask.NameToLayer("Agent");
            
            CurrentState = _initialStateConfig != null ? _initialStateConfig.Create() : new GoapState();
        }

        void Start()
        {
            if (_runOnStart) Initialize(CurrentState);
        }

        public void Initialize(GoapState initialState)
        {
            CurrentState = initialState;
            
            //GOALS
            foreach (var goal in _goalList)
            {
                _goals.Add(goal.Create());
            }
            SortGoals();

            //ACTIONS
            foreach (var action in _actionList)
            {
                _actions.Add(action.Create());
            }
            
            StartCoroutine(PlanGenerator());
        }

        //COROUTINES
        private IEnumerator PlanGenerator()
        {
            while (true)
            {
                Debug.Log("Estado actual: " + CurrentState);
                var id = CreateNewPlan(CurrentState);
                //If plan found.
                if (id >= 0)
                {
                    StartCoroutine(PlanExecution());
                    yield return new WaitUntil(() => !_hasPlan && _active);
                }
                //If no plan found.
                else
                {
                    Debug.LogWarning("No se ha encontrado plan asequible" + " | Estado actual: " + CurrentState);
                    _remainingSeconds = _rePlanSeconds;
                    while (_remainingSeconds > 0)
                    {
                        _remainingSeconds -= Time.deltaTime;
                        yield return null;
                    }
                }
            }
        }

        private IEnumerator PlanExecution()
        {
            _hasPlan = true;
            Interrupted = false;
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            
            GoapState nextState;
            do
            {
                PerformingAction = true;
                
                var task = _currentPlan.ExecuteNext(CurrentState);
                while (!task.IsCompleted) yield return null;
                nextState = task.Result;
                
                if (nextState == null) PerformingAction = false;
                else
                {
                    if (!Interrupted) CurrentState = nextState;
                }

                //Seconds awaited learning.
                if (_learningConfig)
                {
                    _learningConfig.UpdateLearning(_currentPlan.CurrentNode, _currentPlan.InitialState, -(float)stopwatch.ElapsedMilliseconds / 1000f);
                }
                                    
                stopwatch.Restart();
            } while (nextState != null && !Interrupted);
            stopwatch.Stop();
            
            //Plan performance learning
            if (_learningConfig)
            {
                var reward = _currentPlan.IsDone ? _learningConfig.PositiveReward : -_learningConfig.NegativeReward;
                var initialSign = Math.Sign(reward);
                var decay = reward > 0 ? -_learningConfig.PositiveRewardDecay : _learningConfig.NegativeRewardDecay;
                
                foreach (var node in _currentPlan.ExecutedNodes)
                {
                    _learningConfig.UpdateLearning(node, _currentPlan.InitialState, reward);
                    reward += decay;
                    if (Math.Sign(reward) != initialSign) break;
                }
            }
            
            _hasPlan = false;
        }

        //INTERFACE CLASSES
        public void AddAction(GoapAction action) => _actions.Add(action);
        public void AddActions(List<GoapAction> actionList) => _actions.AddRange(actionList);
        public void RemoveAction(GoapAction action) => _actions.Remove(action);

        public void AddGoal(IGoapGoal goal)
        {
            _goals.Add(goal);
            SortGoals();
        }

        public void AddGoals(List<IGoapGoal> goalList)
        {
            _goals.AddRange(goalList);
            SortGoals();
        }

        private void SortGoals() => _goals.Sort((g1, g2) => g2.PriorityLevel.CompareTo(g1.PriorityLevel));

        public int CreateNewPlan(GoapState worldGoapState)
        {
            if (_goals == null || _actions.Count == 0) return -1;
            var i = 0;
            var created = false;
            while (i < _goals.Count && !created)
            {
                _currentGoal = _goals[i];
                created = CreatePlan(worldGoapState, _currentGoal);
                i++;
            }

            if (!created) return -1;
            return i - 1;
        }

        public bool CreatePlan(GoapState state, IGoapGoal goal)
        {
            var generator = new AStar(state, _learningConfig);
            var planner = new BackwardPlanner(generator, this);

            var plan = planner.CreatePlan(state, goal, _actions);
                
            DebugLogs(DebugRecord.GetRecords());
            
            if(_learningConfig) _learningConfig.DebugLearning();
            if (plan == null)
            {
                Debug.Log("[GOAP] Plan not found for: " + goal.Name);
                return false;
            }
            _currentPlan = plan;
            return true;
        }
        
        [ContextMenu("Interrupt")]
        public void Interrupt()
        {
            Interrupted = false;
            
            //If current plan
            if (_hasPlan)
            {
                //If already accomplished
                if (_currentGoal.IsGoal(CurrentState))
                {
                    Interrupted = true;
                    _currentPlan.Interrupt(true);
                }
                //If no longer accomplish the conditions for the current goal
                else if (!_currentPlan.CurrentNode.IsGoal(CurrentState))
                {
                    Interrupted = true;
                    _currentPlan.Interrupt(false);
                }
            }
            //If no current plan
            else
            {
                _remainingSeconds = 0f;
            }
        }
        
        //Debug
        private void DebugLogs(List<string> logs)
        {
            foreach (var log in logs)
            {
                Debug.Log(log);
            }
        }
    }
}