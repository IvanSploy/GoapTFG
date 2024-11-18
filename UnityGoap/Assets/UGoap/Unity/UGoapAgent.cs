using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UGoap.Base;
using UGoap.Learning;
using UGoap.Planner;
using UGoap.Unity.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
using static UGoap.Base.UGoapPropertyManager;
using Debug = UnityEngine.Debug;

namespace UGoap.Unity
{
    [RequireComponent(typeof(Rigidbody))]
    public class UGoapAgent : MonoBehaviour, IGoapAgent
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Renderer _renderer;
        
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
        [SerializeField] private float _speed = 5;
        
        //Agent Properties
        public string Name { get; set; }
        public bool PerformingAction { get; set; }
        public bool Interrupted { get; set; }
        public GoapState CurrentState { get; set; }

        // Start is called before the first frame update
        private void Awake()
        {
            if(!_rigidbody) _rigidbody = GetComponent<Rigidbody>();
            if(!_renderer) _renderer = GetComponentInChildren<Renderer>();
            gameObject.layer = LayerMask.NameToLayer("Agent");
            
            CurrentState = _initialStateConfig != null ? _initialStateConfig.Create() : new GoapState();
            UpdateTag();
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

        public void Interrupt(float seconds = 0)
        {
            throw new NotImplementedException();
        }

        public bool CreatePlan(GoapState state, IGoapGoal goal)
        {
            var generator = new AStar(state, _learningConfig);
            var planner = new AStarPlanner(generator, this);

            var plan = planner.CreatePlan(state, goal, _actions);
                
            DebugLogs(DebugRecord.GetRecords());
            
            if(_learningConfig) _learningConfig.DebugLearning();
            if (plan == null)
            {
                Debug.Log("Plan no encontrado para objetivo: " + goal.Name);
                return false;
            }
            _currentPlan = plan;
            return true;
        }
        
        public void Complete()
        {
            PerformingAction = false;
        }
        
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

            //If interruption worked
            if (Interrupted)
            {
                Debug.Log("INTERRUPTED");
                StopAction();
                //Properties that need to be restored.
                CurrentState.Set(PropertyKey.MoveState, "Ready");
                Complete();
            }
        }

        //ACTIONS
        public bool ValidateGeneric(string actionName, GoapState state)
        {
            bool accomplished = true;
            switch (actionName)
            {
                case "OpenDoor":
                    UGoapEntity entityDoor = UGoapWMM.Get("Door").Object;
                    if (entityDoor.CurrentState.TryGetOrDefault(PropertyKey.DoorState, "Opened") == "Locked")
                    {
                        CurrentState.Set(PropertyKey.DoorState, "Locked");
                        accomplished = false;
                    }
                    break;
                case "UnlockDoor":
                    break;
                case "GetKey":
                    break;
                case "Tag":
                    var isIt = CurrentState.TryGetOrDefault(PropertyKey.IsIt, true);
                    if (isIt)
                    {
                        CurrentState.Set(PropertyKey.MoveState, "Ready");
                        accomplished = false;
                    }
                    break;
                default:
                    break;
            }
            
            return accomplished;
        }
        
        public void GoGenericAction(string actionName, ref GoapState state)
        {
            switch (actionName)
            {
                case "OpenDoor":
                case "UnlockDoor":
                    UGoapEntity entityLockedDoor = UGoapWMM.Get("Door").Object;
                    entityLockedDoor.GetComponent<Animator>()?.SetBool("Opened", true);
                    break;
                case "GetKey":
                    UGoapEntity entityKey = UGoapWMM.Get("Key").Object;
                    Destroy(entityKey.gameObject);
                    break;
                case "SetPlayerDestination":
                    UGoapEntity entityPlayer = UGoapWMM.Get("Player").Object;
                    state.Set(PropertyKey.DestinationX, entityPlayer.transform.position.x);
                    state.Set(PropertyKey.DestinationZ, entityPlayer.transform.position.z);
                    break;
                case "MoveToDestination":
                    var x = state.TryGetOrDefault(PropertyKey.DestinationX, 0f);
                    var z = state.TryGetOrDefault(PropertyKey.DestinationZ, 0f);
                    GoTo(new Vector3(x, transform.position.y, z), 1);
                    return; //Wait involves modification of performing action.
                case "Tag":
                    SetTag(false);
                    Debug.LogError("SE HA APLICADO LA ACCIÓN TAG, ESTO NO DEBERÍA OCURRIR EN TEORÍA");
                    break;
                default:
                    break;
            }
            
            Complete();
        }
        
        public void GoTo(string target, float speedFactor)
        {
            GoTo(UGoapWMM.Get(target).Position, _speed * speedFactor);
        }
        
        public void GoTo(Vector3 target, float speedFactor)
        {
            StartAction(Movement(target, _speed * speedFactor));
        }
        
        //COROUTINES
        private IEnumerator Movement(Vector3 target, float vel)
        {
            bool reached = false;
            while (!reached)
            {
                var t = transform;
                var p = t.position;
                target.y = p.y;
                t.position = Vector3.MoveTowards(p, target, Time.deltaTime * vel);
                t.rotation = Quaternion.LookRotation(target - p, Vector3.up);
                Vector3 aux = target;
                aux.y = p.y;
                if (Vector3.Distance(transform.position, aux) < float.Epsilon)
                {
                    reached = true;
                }
                yield return null;
            }

            Complete();
        }
        
        private void StartAction(IEnumerator routine)
        {
            _currentActionRoutine = StartCoroutine(routine);
        }
        
        private void StopAction()
        {
            if (_currentActionRoutine != null)
            {
                StopCoroutine(_currentActionRoutine);
                _currentActionRoutine = null;
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

        private void UpdateTag()
        {
            bool isIt = CurrentState.TryGetOrDefault(PropertyKey.IsIt, false);
            SetTag(isIt);
        }

        private void SetTag(bool isIt)
        {
            _renderer.material.color = isIt ? Color.red : Color.cyan;
        }

        private void Update()
        {
            UGoapEntity entityPlayer = UGoapWMM.Get("Player").Object;
            bool near = Vector3.Distance(entityPlayer.transform.position, transform.position) <= 3f;
            bool previousNear = CurrentState.TryGetOrDefault(PropertyKey.PlayerNear, false);
            
            if (near != previousNear)
            {
                CurrentState.Set(PropertyKey.PlayerNear, near);
                if(near) Interrupt();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var tag = CurrentState.TryGetOrDefault(PropertyKey.IsIt, false);
            tag = !tag;
            CurrentState.Set(PropertyKey.IsIt, tag);
            UpdateTag();
            Debug.Log("COLLISION GOAP");
            Interrupt();
        }
    }
}