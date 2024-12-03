using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UGoap.Base;
using UGoap.Learning;
using UGoap.Planner;
using UGoap.Unity.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace UGoap.Unity
{
    public class UGoapAgent : MonoBehaviour, IGoapAgent
    {
        [Header("Planner")]
        [SerializeField] private bool _runOnStart;
        [SerializeField] private bool _active = true;
        private float _rePlanCooldown;
        [SerializeField] private float _rePlanSeconds = 5;
        [SerializeField] private StateConfig _initialStateConfig;
        [SerializeField] private List<PriorityGoal> _goalList;
        [SerializeField] private List<ActionConfig> _actionList;
        [SerializeField] private LearningConfig _learningConfig;
        
        [FormerlySerializedAs("ThinkTime")]
        [Header("View")]
        [Tooltip("Time that simulates that agent is thinking.")]
        public float IndicatorTime = 0.5f;
        [Tooltip("Meters/Seconds moved by the agent.")]
        public float Speed = 5;
        
        //Agent base related
        private bool _hasPlan;
        private Plan _currentPlan;
        private readonly List<IGoapGoal> _goals = new();
        private readonly List<GoapAction> _actions = new();
        private IGoapGoal _currentGoal;

        private Planner.Planner _planner;
        
        //Agent Properties
        public string Name { get; set; }
        public bool Interrupted { get; set; }
        public GoapState CurrentState { get; set; }

        //Events
        public event System.Action PlanningStarted;
        public event System.Action PlanningEnded;
        public event System.Action PlanAchieved;
        public event System.Action PlanFailed;

        // Start is called before the first frame update
        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Agent");
            CurrentState = _initialStateConfig != null ? _initialStateConfig.Create() : new GoapState();
            
            //Creation of planner
            var generator = new AStar();
            if(_learningConfig) generator.SetLearning(_learningConfig);
            _planner = new BackwardPlanner(generator, this);
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
                
                //Simular pensamiento.
                PlanningStarted?.Invoke();
                yield return new WaitForSeconds(IndicatorTime);
                
                var id = CreateNewPlan(CurrentState);
                PlanningEnded?.Invoke();
                //If plan found.
                if (id >= 0)
                {
                    yield return StartCoroutine(PlanExecution());
                    yield return new WaitUntil(() => _active);
                }
                //If no plan found.
                else
                {
                    Debug.LogWarning("No se ha encontrado plan asequible" + " | Estado actual: " + CurrentState);
                    _rePlanCooldown = _rePlanSeconds;
                    while (_rePlanCooldown > 0)
                    {
                        _rePlanCooldown -= Time.deltaTime;
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
            
            GoapState nextState = CurrentState;
            do
            {
                var task = _currentPlan.ExecuteNext(CurrentState);
                if (task != null)
                {
                    while (!task.IsCompleted) yield return null;
                    nextState = task.Result;
                }
                else
                {
                    nextState = null;
                }

                if (nextState != null)
                {
                    if (!Interrupted) CurrentState = nextState;
                }

                //Seconds awaited learning.
                if (_learningConfig)
                {
                    _learningConfig.UpdateLearning(_currentPlan.Current, _currentPlan.Next, _currentPlan.InitialState, -(float)stopwatch.ElapsedMilliseconds / 1000f);
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

                var nodes = _currentPlan.ExecutedActions.ToList();
                for (var i = 0; i < nodes.Count - 1; i++)
                {
                    var node = nodes[i+1];
                    var nextNode = nodes[i];
                    _learningConfig.UpdateLearning(node, nextNode, _currentPlan.InitialState,
                        reward);
                    reward += decay;
                    if (Math.Sign(reward) != initialSign) break;
                }
            }

            if (_currentPlan.IsDone)
            {
                //Simulate victory.
                PlanAchieved?.Invoke();
            }
            else
            {
                PlanFailed?.Invoke();
            }
            yield return new WaitForSeconds(IndicatorTime);
            
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

        public int CreateNewPlan(GoapState initialState)
        {
            if (_goals == null || _actions.Count == 0) return -1;

            for (int i = 0; i < _goals.Count; i++)
            {
                _currentGoal = _goals[i];
                var found = CreatePlan(initialState, _currentGoal);
                if (found) return i;
            }

            return -1;
        }

        public bool CreatePlan(GoapState initialState, IGoapGoal goal)
        {
            var plan = _planner.CreatePlan(initialState, goal, _actions);
                
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

        [ContextMenu("ForceInterrupt")]
        public void ForceInterrupt()
        {
            Interrupted = true;
            _currentPlan.Interrupt();
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
                    _currentPlan.Interrupt();
                    _currentPlan.IsDone = true;
                }
                //TODO: If no longer accomplish the conditions for the current goal INCOMPLETE, CHECK FULL PLAN.
                else if (!_currentPlan.Current.Goal.CheckConflict(CurrentState))
                {
                    Interrupted = true;
                    _currentPlan.Interrupt();
                }
            }
            //If no current plan
            else
            {
                _rePlanCooldown = 0f;
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

        private void OnDestroy()
        {
            _currentPlan.Interrupt();
        }
    }
}