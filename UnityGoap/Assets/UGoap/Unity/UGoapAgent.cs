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
        private bool Colliding = false;
        
        public string Name { get; set; }
        public bool PerformingAction { get; set; }
        
        public bool active = true;
        public bool wait = true;
        public float speed = 5;
        public float replanSeconds = 5;
        
        public bool hasPlan;

        //Agent base related
        private Plan _currentPlan;
        private readonly List<IGoapGoal> _goals = new();
        private readonly List<IGoapAction> _actions = new();
        private IGoapGoal _currentGoal;
        
        public GoapState CurrentGoapState { get; set; }

        // Start is called before the first frame update

        private void Awake()
        {
            _rigidbody ??= GetComponent<Rigidbody>(); 
            gameObject.layer = LayerMask.NameToLayer("Agent");
            
            CurrentGoapState = _initialState != null ? _initialState.Create() : new();
        }

        void Start()
        {
            if (_runOnStart) Initialize(CurrentGoapState);
        }

        public void Initialize(GoapState initialState)
        {
            CurrentGoapState = initialState;
            
            //OBJETIVOS
            foreach (var goal in _goalObjects)
            {
                _goals.Add(goal.Create());
            }
            SortGoals();

            //ACCIONES
            foreach (var action in _actionObjects)
            {
                _actions.Add(action);
            }
            
            StartCoroutine(PlanCreator());
        }

        private IEnumerator PlanCreator()
        {
            while (true)
            {
                Debug.Log("Estado actual: " + CurrentGoapState);
                var id = CreateNewPlan(CurrentGoapState);
                if (id >= 0)
                {
                    
                    StartCoroutine(PlanExecute());
                    yield return new WaitUntil(() => !hasPlan && active);
                }
                else
                {
                    Debug.LogWarning("No se ha encontrado plan asequible" + " | Estado actual: " +
                                     CurrentGoapState);
                    yield return new WaitForSeconds(replanSeconds);
                }
            }
        }

        //CORRUTINAS
        private IEnumerator PlanExecute()
        {
            hasPlan = true;
            GoapState result;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            do
            {
                result = _currentPlan.PlanStep(CurrentGoapState);
                if (result != null){ CurrentGoapState = result;}
                yield return new WaitWhile(() => PerformingAction);

                if (_goapQLearning)
                {
                    _goapQLearning.UpdateLearning(_currentPlan.CurrentNode, _currentPlan.InitialState, -(float)stopwatch.ElapsedMilliseconds / 1000f);
                }
                                    
                stopwatch.Restart();
            } while (result != null);
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
            
            hasPlan = false;
        }

        //INTERFACE CLASSES
        public void AddAction(IGoapAction action) => _actions.Add(action);
        public void AddActions(List<IGoapAction> actionList) => _actions.AddRange(actionList);

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

        //ACTIONS
        public bool ValidateGeneric(string actionName, GoapState state)
        {
            bool accomplished = true;
            switch (actionName)
            {
                case "OpenDoor":
                    UGoapEntity entityDoor = UGoapWMM.Get("Door").Object;
                    if (entityDoor.CurrentGoapState.TryGetOrDefault(UGoapPropertyManager.PropertyKey.DoorState, "Opened") == "Locked")
                    {
                        CurrentGoapState.Set(UGoapPropertyManager.PropertyKey.DoorState, "Locked");
                        accomplished = false;
                    }
                    break;
                case "UnlockDoor":
                    break;
                case "GetKey":
                    break;
                case "Tag":
                    var isIt = CurrentGoapState.TryGetOrDefault(UGoapPropertyManager.PropertyKey.IsIt, true);
                    if (isIt && !Colliding)
                    {
                        CurrentGoapState.Set(UGoapPropertyManager.PropertyKey.MoveState, "Ready");
                        accomplished = false;
                    }
                    break;
                default:
                    break;
            }
            
            return accomplished;
        }
        
        public void GoGenericAction(string actionName, GoapState state, float seconds)
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
                    state.Set(UGoapPropertyManager.PropertyKey.DestinationX, entityPlayer.transform.position.x);
                    state.Set(UGoapPropertyManager.PropertyKey.DestinationZ, entityPlayer.transform.position.z);
                    break;
                /*case "SetDestination55":
                    state.Set(UGoapPropertyManager.PropertyKey.DestinationX, 5f);
                    state.Set(UGoapPropertyManager.PropertyKey.DestinationZ, 5f);
                    break;
                case "SetDestination-55":
                    state.Set(UGoapPropertyManager.PropertyKey.DestinationX, -5f);
                    state.Set(UGoapPropertyManager.PropertyKey.DestinationZ, 5f);
                    break;
                case "SetDestination5-5":
                    state.Set(UGoapPropertyManager.PropertyKey.DestinationX, 5f);
                    state.Set(UGoapPropertyManager.PropertyKey.DestinationZ, -5f);
                    break;
                case "SetDestination-5-5":
                    state.Set(UGoapPropertyManager.PropertyKey.DestinationX, -5f);
                    state.Set(UGoapPropertyManager.PropertyKey.DestinationZ, -5f);
                    break;*/
                case "MoveToDestination":
                    var x = state.TryGetOrDefault(UGoapPropertyManager.PropertyKey.DestinationX, 0f);
                    var z = state.TryGetOrDefault(UGoapPropertyManager.PropertyKey.DestinationZ, 0f);
                    GoTo(new Vector3(x, transform.position.y, z), 1);
                    return; //Wait involves modification of performing action.
                default:
                    break;
            }
            
            if(wait) StartCoroutine(Wait(seconds));
        }
        
        public void GoTo(string target, float speedFactor)
        {
            StartCoroutine(Movement(speed * speedFactor, UGoapWMM.Get(target).Position));
        }
        
        public void GoTo(Vector3 target, float speedFactor)
        {
            StartCoroutine(Movement(speed * speedFactor, target));
        }
        
        //COROUTINES
        private IEnumerator Movement(float vel, Vector3 target)
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

            PerformingAction = false;
        }

        
        IEnumerator Wait(float seconds)
        {
            PerformingAction = true;
            yield return new WaitForSeconds(seconds);
            PerformingAction = false;
        }
        
        //Debug
        private void DebugLogs(List<string> logs)
        {
            foreach (var log in logs)
            {
                Debug.Log(log);
            }
        }

        private void Update()
        {
            UGoapEntity entityPlayer = UGoapWMM.Get("Player").Object;
            bool near = Vector3.Distance(entityPlayer.transform.position, transform.position) <= 3f;
            CurrentGoapState.Set(UGoapPropertyManager.PropertyKey.PlayerNear, near);
        }

        private void OnTriggerEnter(Collider other)
        {
            var tag = CurrentGoapState.TryGetOrDefault(UGoapPropertyManager.PropertyKey.IsIt, false);
            tag = !tag;
            CurrentGoapState.Set(UGoapPropertyManager.PropertyKey.IsIt, tag);
            Colliding = true;
        }

        private void OnCollisionStay(Collision other)
        {
            Colliding = true;
        }

        private void OnTriggerExit(Collider other)
        {
            Colliding = false;
        }
    }
}