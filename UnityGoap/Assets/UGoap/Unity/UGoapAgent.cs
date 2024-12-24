using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UGoap.Base;
using UGoap.Learning;
using UGoap.Planning;
using UGoap.Unity.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

namespace UGoap.Unity
{
    public class UGoapAgent : MonoBehaviour, IAgent
    {
        [Header("Planner")]
        [SerializeField] private bool _runOnStart;
        [SerializeField] private bool _active = true;
        [SerializeField] private bool _async;
        [SerializeField] private float _rePlanSeconds = 5;
        [SerializeField] private StateConfig _initialStateConfig;
        [SerializeField] private List<PriorityGoal> _goalList;
        [SerializeField] private List<ActionConfig> _actionList;
        [SerializeField] private bool _useHeuristic;
        
        [FormerlySerializedAs("ThinkTime")]
        [Header("View")]
        [Tooltip("Time that simulates that agent is thinking.")]
        public float IndicatorTime = 0.5f;
        [Tooltip("Meters/Seconds moved by the agent.")]
        public float Speed = 5;
        
        //Agent base related
        private readonly List<IGoal> _goals = new();
        private readonly List<Base.Action> _actions = new();
        private Planner _planner;
        private QLearning _qLearning;
        
        private bool _hasPlan;
        private Plan _currentPlan;
        private float _rePlanCooldown;
        
        //Agent Properties
        public string Name { get; set; }
        public bool Interrupted { get; set; }
        public State CurrentState { get; set; }
        public IGoal CurrentGoal { get; set; }

        //Events
        public event System.Action PlanningStarted;
        public event System.Action PlanningEnded;
        public event System.Action PlanAchieved;
        public event System.Action PlanFailed;

        // Start is called before the first frame update
        public void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Agent");
            CurrentState = _initialStateConfig != null ? _initialStateConfig.Create() : new State();
            _planner = CreatePlanner();
        }

        protected virtual Planner CreatePlanner()
        {
            var generator = new AStar();
            if (_useHeuristic) generator.SetHeuristic(UGoapData.GetCustomHeuristic());
            return new BackwardPlanner(generator, this);
        }

        void Start()
        {
            if (_runOnStart) Initialize(CurrentState);
        }
        
        void OnDestroy()
        {
            _currentPlan?.Interrupt();
        }

        public void Initialize(State initialState)
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

            if (_async)
            {
                StartCoroutine(PlanGeneratorAsync());
            }
            else
            {
                StartCoroutine(PlanGenerator());
            }
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
                
                var id = CreatePlan(CurrentState);
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
        
        private IEnumerator PlanGeneratorAsync()
        {
            while (true)
            {
                Debug.Log("Estado actual: " + CurrentState);
                
                //Simular pensamiento.
                PlanningStarted?.Invoke();
                
                var planTask = CreatePlanAsync(CurrentState);
                while (!planTask.IsCompleted) yield return null;
                
                PlanningEnded?.Invoke();
                //If plan found.
                if (planTask.Result >= 0)
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
            
            State nextState;
            do
            {
                nextState = null;
                var task = _currentPlan.ExecuteNext(this);
                if (task != null)
                {
                    while (!task.IsCompleted) yield return null;
                    if (!Interrupted)
                    {
                        CurrentState = nextState = task.Result;
                    }
                }
                                    
            } while (nextState != null && !_currentPlan.IsCompleted && !Interrupted);

            if (_currentPlan.IsCompleted) PlanAchieved?.Invoke();
            else PlanFailed?.Invoke();
            yield return new WaitForSeconds(IndicatorTime);
            
            _hasPlan = false;
        }

        //INTERFACE CLASSES
        public void AddAction(Base.Action action) => _actions.Add(action);
        public void AddActions(List<Base.Action> actionList) => _actions.AddRange(actionList);
        public void RemoveAction(Base.Action action) => _actions.Remove(action);

        public void AddGoal(IGoal goal)
        {
            _goals.Add(goal);
            SortGoals();
        }

        public void AddGoals(List<IGoal> goalList)
        {
            _goals.AddRange(goalList);
            SortGoals();
        }

        private void SortGoals() => _goals.Sort((g1, g2) => g2.PriorityLevel.CompareTo(g1.PriorityLevel));

        public int CreatePlan(State initialState)
        {
            if (_goals == null || _actions.Count == 0) return -1;

            for (int i = 0; i < _goals.Count; i++)
            {
                CurrentGoal = _goals[i];
                var found = CreatePlanForGoal(initialState, CurrentGoal);
                if (found) return i;
            }

            return -1;
        }

        public bool CreatePlanForGoal(State initialState, IGoal goal)
        {
            var plan = _planner.CreatePlan(initialState, goal, _actions);
            
            DebugLogs(DebugRecord.GetRecords());
            
            if (plan == null)
            {
                Debug.Log("[GOAP] Plan not found for: " + goal.Name);
                return false;
            }
            _currentPlan = plan;
            return true;
        }
        
        public async Task<int> CreatePlanAsync(State initialState)
        {
            if (_goals == null || _actions.Count == 0) return -1;

            for (int i = 0; i < _goals.Count; i++)
            {
                CurrentGoal = _goals[i];
                var found = await CreatePlanForGoalAsync(initialState, CurrentGoal);
                if (found) return i;
            }

            return -1;
        }

        public async Task<bool> CreatePlanForGoalAsync(State initialState, IGoal goal)
        {
            _currentPlan = await _planner.CreatePlanAsync(initialState, goal, _actions);
            
            DebugLogs(DebugRecord.GetRecords());
            
            if (_currentPlan == null)
            {
                Debug.Log("[GOAP] Plan not found for: " + goal.Name);
                return false;
            }
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
                if (CurrentGoal.IsGoal(CurrentState))
                {
                    Interrupted = true;
                    _currentPlan.Interrupt();
                    _currentPlan.IsCompleted = true;
                }
                else if (!_currentPlan.VerifyCurrent(this))
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
    }
}