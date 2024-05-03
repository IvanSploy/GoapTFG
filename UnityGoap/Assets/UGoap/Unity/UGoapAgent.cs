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
        [SerializeField] private bool _runOnStart;
        [SerializeField] private UGoapState _initialState;
        [SerializeField] private GoapQLearning _goapQLearning;
        [SerializeField] private List<PriorityGoal> _goalObjects;
        [SerializeField] private List<UGoapAction> _actionObjects;
        
        [SerializeField] private Rigidbody _rigidbody;
        
        public string Name { get; set; }
        
        public bool active = true;
        public bool wait = true;
        public float speed = 5;
        public float replanSeconds = 5;
        
        public bool hasPlan;
        public bool performingAction;

        //Agent base related
        private Plan _currentPlan;
        private List<IGoapGoal> _goals;
        private List<IGoapAction> _actions;
        private IGoapGoal _currentGoal;
        
        public GoapState CurrentGoapState { get; set; }

        // Start is called before the first frame update

        private void Awake()
        {
            _rigidbody ??= GetComponent<Rigidbody>(); 
            gameObject.layer = LayerMask.NameToLayer("Agent");
        }

        void Start()
        {
            _goals = new();
            _actions = new();
            CurrentGoapState = _initialState != null ? _initialState.Create() : new();
            
            //OBJETIVOS
            foreach (var goal in _goalObjects)
            {
                _goals.Add(goal.Create());
            }

            //ACCIONES
            foreach (var action in _actionObjects)
            {
                _actions.Add(action);
            }

            SortGoals();

            if (_runOnStart) Initialize(CurrentGoapState);
        }

        public void Initialize(GoapState initialState)
        {
            CurrentGoapState = initialState;
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
                yield return new WaitWhile(() => performingAction);

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
                        if (state != null)
                        {
                            state.Set(UGoapPropertyManager.PropertyKey.DoorState, "Locked");
                            CurrentGoapState = state;
                            accomplished = false;
                        }
                    }
                    break;
                case "UnlockDoor":
                    break;
                case "GetKey":
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
                default:
                    break;
            }
            
            if(wait) StartCoroutine(Wait(seconds));
        }
        
        public void GoToTarget(string target, float speedFactor)
        {
            StartCoroutine(Movement(speed * speedFactor, UGoapWMM.Get(target).Position));
        }
        
        //COROUTINES
        private IEnumerator Movement(float vel, Vector3 target)
        {
            performingAction = true;
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
    }
}