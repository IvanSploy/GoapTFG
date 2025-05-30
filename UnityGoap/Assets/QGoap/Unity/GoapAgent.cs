using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using QGoap.Base;
using QGoap.Learning;
using QGoap.Planning;
using QGoap.Unity.ScriptableObjects;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace QGoap.Unity
{
    public class GoapAgent : MonoBehaviour, IAgent
    {
        [Header("Planner")]
        [SerializeField] private bool _runOnStart;
        [SerializeField] private bool _active = true;
        [SerializeField] private bool _async;
        [SerializeField] private bool _greedy;
        [SerializeField] private float _rePlanSeconds = 5;
        [SerializeField] private StateConfig _initialStateConfig;
        [SerializeField] private List<GoalConfig> _goalList;
        [SerializeField] private List<ActionBaseConfig> _actionList;
        
        [Header("View")]
        public float IndicatorTime = 0.5f;
        [Tooltip("Meters/Seconds moved by the agent.")]
        public float Speed = 5;
        
        //Agent base related
        private readonly List<Goal> _goals = new();
        private readonly List<Base.Action> _actions = new();
        private Planner _planner;
        
        private bool _hasPlan;
        private Plan _currentPlan;
        private float _rePlanCooldown;
        private float _interruptTime;
        private Coroutine _planCoroutine;
        
        //Agent Properties
        public string Name => name;
        public bool IsInterrupted { get; private set; }
        public State CurrentState { get; private set; }
        public Goal CurrentGoal { get; private set; }
        public NodeAction? PreviousAction => _currentPlan.Previous;
        public NodeAction CurrentAction => _currentPlan.Current;
        public bool IsCompleted => _currentPlan?.IsCompleted ?? false;

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
        }

        void Start()
        {
            if (_runOnStart)
            {
                Initialize(CurrentState);
            }
        }
        
        public void OnDestroy()
        {
            _currentPlan?.Interrupt();
            foreach (var action in _actions)
            {
                if(action is LearningAction learningAction)
                {
                    learningAction.Save();
                }
            }
        }
        
        protected virtual Planner CreatePlanner()
        {
            var generator = new AStar();
            return new BackwardPlanner(generator, this, _greedy);
        }

        public void Initialize(State initialState)
        {
            CurrentState = initialState;
            _planner = CreatePlanner();
            
            //GOALS
            foreach (var goal in _goalList)
            {
                var goalCopy = Instantiate(goal);
                goalCopy.name = goal.name;
                _goals.Add(goalCopy.Create());
            }

            //ACTIONS
            foreach (var action in _actionList)
            {
                var actionCopy = Instantiate(action);
                actionCopy.name = action.name;
                _actions.Add(actionCopy.Create(this));
            }

            StartPlanning();
        }

        private void StartPlanning()
        {
            if (_planCoroutine != null)
            {
                StopCoroutine(_planCoroutine);
                _planCoroutine = null;
            }
            _planCoroutine = StartCoroutine(_async ? PlanGeneratorAsync() : PlanGenerator());
        }

        //COROUTINES
        private IEnumerator PlanGenerator()
        {
            while (true)
            {
                if (IsInterrupted)
                {
                    if(_interruptTime > 0) yield return new WaitForSeconds(_interruptTime);
                    _interruptTime = 0;
                    _rePlanCooldown = 0;
                }
                
                Debug.Log("Estado actual: " + CurrentState);
                
                //Simular pensamiento.
                PlanningStarted?.Invoke();
                if(IndicatorTime > 0) yield return new WaitForSeconds(IndicatorTime);
                
                var id = CreatePlan(CurrentState);
                PlanningEnded?.Invoke();
                //If plan found.
                if (id >= 0 && _currentPlan.Count > 0)
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
                if (IsInterrupted)
                {
                    if(_interruptTime > 0) yield return new WaitForSeconds(_interruptTime);
                    _interruptTime = 0;
                    _rePlanCooldown = 0;
                }
                
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
            IsInterrupted = false;
            
            do
            {
                var task = _currentPlan.ExecuteNext();
                while (!task.IsCompleted) yield return null;
                if (task.Result == null)
                {
                    _currentPlan.Finish(null);
                    break;
                }
                State result = CurrentState + task.Result;
                _currentPlan.Finish(result);
                if (!IsInterrupted)
                {
                    CurrentState = result;
                }
            } while (!_currentPlan.IsCompleted && !IsInterrupted);

            if (_currentPlan.IsCompleted) PlanAchieved?.Invoke();
            else PlanFailed?.Invoke();
            
            _hasPlan = false;
        }

        //INTERFACE CLASSES
        public void AddAction(Base.Action action) => _actions.Add(action);
        public void AddActions(List<Base.Action> actionList) => _actions.AddRange(actionList);
        public void RemoveAction(Base.Action action) => _actions.Remove(action);

        public void AddGoal(Goal goal)
        {
            _goals.Add(goal);
        }

        public void AddGoals(List<Goal> goalList)
        {
            _goals.AddRange(goalList);
        }

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

        public bool CreatePlanForGoal(State initialState, Goal goal)
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

        public async Task<bool> CreatePlanForGoalAsync(State initialState, Goal goal)
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
        public void ForceInterruptEditor()
        {
            ForceInterrupt();
        }
        
        [ContextMenu("Interrupt")]
        public void InterruptEditor()
        {
            Interrupt();
        }

        public void ForceInterrupt(float seconds = 0)
        {
            _interruptTime = seconds;
            IsInterrupted = true;
            _currentPlan?.Interrupt();
            if (seconds > 0) StartPlanning();
        }
        
        public void Interrupt(float seconds = 0)
        {
            IsInterrupted = false;
            
            //If current plan
            if (_hasPlan)
            {
                //If already accomplished
                if (CurrentGoal.IsGoal(CurrentState))
                {
                    _currentPlan.IsCompleted = true;
                    ForceInterrupt(seconds);
                }
                else if (!_currentPlan.VerifyCurrent())
                {
                    ForceInterrupt(seconds);
                }
            }
            //If no current plan
            else
            {
                _rePlanCooldown = seconds;
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