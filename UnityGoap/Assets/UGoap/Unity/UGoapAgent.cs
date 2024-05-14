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
        [SerializeField] private bool _runOnStart;
        [SerializeField] private UGoapState _initialState;
        [SerializeField] private GoapQLearning _goapQLearning;
        [SerializeField] private List<PriorityGoal> _goalObjects;
        [SerializeField] private List<UGoapAction> _actionObjects;
        
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Renderer _renderer;

        public string Name { get; set; }
        public bool PerformingAction { get; set; }
        public bool Interrupted { get; set; }
        
        [FormerlySerializedAs("active")] public bool Active = true;
        [FormerlySerializedAs("speed")] public float Speed = 5;
        [FormerlySerializedAs("replanSeconds")] public float ReplanSeconds = 5;
        [FormerlySerializedAs("hasPlan")] public bool HasPlan;

        //Agent base related
        private Plan _currentPlan;
        private readonly List<IGoapGoal> _goals = new();
        private readonly List<IGoapAction> _actions = new();
        private IGoapGoal _currentGoal;
        [Header("Debug")]
        [SerializeField] private float _remainingSeconds;
        private Coroutine _currentActionRoutine;
        
        //States of agent
        public GoapState CurrentState { get; set; }
        private GoapState NextState;

        // Start is called before the first frame update
        private void Awake()
        {
            if(!_rigidbody) _rigidbody = GetComponent<Rigidbody>();
            if(!_renderer) _renderer = GetComponentInChildren<Renderer>();
            gameObject.layer = LayerMask.NameToLayer("Agent");
            
            CurrentState = _initialState != null ? _initialState.Create() : new();
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
            foreach (var goal in _goalObjects)
            {
                _goals.Add(goal.Create());
            }
            SortGoals();

            //ACTIONS
            foreach (var action in _actionObjects)
            {
                _actions.Add(action);
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
                    yield return new WaitUntil(() => !HasPlan && Active);
                }
                //If no plan found.
                else
                {
                    Debug.LogWarning("No se ha encontrado plan asequible" + " | Estado actual: " + CurrentState);
                    _remainingSeconds = ReplanSeconds;
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
            Interrupted = false;
            HasPlan = true;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                if(PerformingAction)
                    Debug.LogError("SE ESTABA EJECUTANDO UNA ACCIÓN");
                
                PerformingAction = true;
                NextState = _currentPlan.PlanStep(CurrentState);
                if (NextState != null)
                {
                    //Using WaitWhile creates race conditions.
                    while (PerformingAction)
                    {
                        yield return null;
                    }
                    if(!Interrupted) CurrentState = NextState;
                }
                else
                {
                    PerformingAction = false;
                }

                if (_goapQLearning)
                {
                    _goapQLearning.UpdateLearning(_currentPlan.CurrentNode, _currentPlan.InitialState, -(float)stopwatch.ElapsedMilliseconds / 1000f);
                }
                                    
                stopwatch.Restart();
            } while (NextState != null && !Interrupted);
            stopwatch.Stop();
            
            if (_goapQLearning)
            {
                var reward = _currentPlan.IsDone ? _goapQLearning.PositiveReward : -_goapQLearning.NegativeReward;
                var initialSign = Math.Sign(reward);
                var decay = reward > 0 ? -_goapQLearning.PositiveRewardDecay : _goapQLearning.NegativeRewardDecay;
                
                foreach (var node in _currentPlan.ExecutedNodes)
                {
                    _goapQLearning.UpdateLearning(node, _currentPlan.InitialState, reward);
                    reward += decay;
                    if (Math.Sign(reward) != initialSign) break;
                }
            }
            
            HasPlan = false;
        }

        //INTERFACE CLASSES
        public void AddAction(IGoapAction action) => _actions.Add(action);
        public void AddActions(List<IGoapAction> actionList) => _actions.AddRange(actionList);
        public void RemoveAction(IGoapAction action) => _actions.Remove(action);

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
            var generator = new AStar(state, _goapQLearning);
            var planner = new GoapPlanner(generator, this);

            var plan = planner.CreatePlan(state, goal, _actions);
                
            DebugLogs(DebugRecord.GetRecords());
            
            if(_goapQLearning) _goapQLearning.DebugLearning();
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
        
        public void Interrupt(float seconds = 0f)
        {
            Interrupted = false;
            
            //If current plan
            if (HasPlan)
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
                if(seconds > 0)
                {
                    PerformingAction = true;
                    StartAction(Wait(seconds));
                }
                else
                {
                    Complete();
                }
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
        
        public void GoGenericAction(string actionName, ref GoapState state, float seconds = 0f)
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
            
            if(seconds > 0) StartAction(Wait(seconds));
            else Complete();
        }
        
        public void GoTo(string target, float speedFactor)
        {
            GoTo(UGoapWMM.Get(target).Position, Speed * speedFactor);
        }
        
        public void GoTo(Vector3 target, float speedFactor)
        {
            StartAction(Movement(target, Speed * speedFactor));
        }
        
        //COROUTINES
        private IEnumerator Movement(Vector3 target, float vel)
        {
            bool reached = false;
            while (!reached)
            {
                var position = transform.position;
                target.y = position.y;
                Vector3 newPos = Vector3.MoveTowards(position, target,
                    Time.deltaTime * vel);
                transform.rotation = Quaternion.LookRotation(target - position, Vector3.up);
                transform.position = newPos;
                Vector3 aux = target;
                aux.y = position.y;
                if (Vector3.Distance(transform.position, aux) < float.Epsilon) reached = true;
                yield return null;
            }

            Complete();
        }
        
        IEnumerator Wait(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Complete();
        }
        
        //Debug
        private Coroutine StartAction(IEnumerator routine)
        {
            _currentActionRoutine = StartCoroutine(routine);
            return _currentActionRoutine;
        }
        
        private void StopAction()
        {
            if (_currentActionRoutine != null)
            {
                StopCoroutine(_currentActionRoutine);
                _currentActionRoutine = null;
            }
        }
        
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
            Debug.Log("COLISIÓN GOAP");
            Interrupt(0.5f);
        }
    }
}