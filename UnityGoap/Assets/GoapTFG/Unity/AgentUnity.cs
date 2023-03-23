using System;
using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Planner;
using GoapTFG.Unity.ScriptableObjects;
using UnityEngine;
using static GoapTFG.Unity.GoapData;
using static GoapTFG.Unity.PropertyManager;
using Random = UnityEngine.Random;

namespace GoapTFG.Unity
{
    public class AgentUnity : MonoBehaviour, IAgent<PropertyList, object>
    {
        [Serializable]
        private struct GoalObject
        {
            [SerializeField] private GoalScriptableObject goal;

            [Range(0, 15)] [SerializeField] private int priority;

            public Goal<PropertyList, object> Create()
            {
                return goal.Create(priority);
            }
        }

        [SerializeField] private List<GoalObject> goalObjects;
        [SerializeField] private List<ActionScriptableObject> actionObjects;
        
        public bool active = true;
        public bool hasPlan;
        public bool performingAction = false;
        public bool regressivePlan = false;
        public float speed = 5;
        

        //Agent base related
        private Stack<Base.Action<PropertyList, object>> _currentPlan;
        private List<Goal<PropertyList, object>> _goals;
        private List<Base.Action<PropertyList, object>> _actions;
        private Goal<PropertyList, object> _currentGoal;
        private PropertyGroup<PropertyList, object> _currentState;

        //Referencias
        public BlackboardData Blackboard;

        // Start is called before the first frame update
        void Start()
        {
            _currentPlan = new();
            _goals = new();
            _actions = new();
            List<Goal<PropertyList, object>> myGoals = new();
            List<GoapTFG.Base.Action<PropertyList, object>> myActions = new();
            _currentState = new();
            
            //OBJETIVOS
            foreach (var goal in goalObjects)
            {
                _goals.Add(goal.Create());
            }

            //ACCION PRINCIPAL
            foreach (var action in actionObjects)
            {
                _actions.Add(action.Create(this));
            }

            OrderGoals();

            //Se crea el blackboard utilizado por las acciones de GOAP.
            Blackboard = new BlackboardData();
            StartCoroutine(Replan());
        }

        //CORRUTINAS
        private IEnumerator Replan()
        {
            while (true)
            {
                Debug.Log("Estado actual: " + _currentState);
                var id = CreateNewPlan(_currentState);
                if (id >= 0)
                {
                    var debugLog = "Acciones para conseguir el objetivo: " + Count() + "\n" + _goals[id];
                    hasPlan = true;
                    foreach (var action in _currentPlan)
                    {
                        debugLog += action.Name + "\n";
                    }

                    Debug.Log(debugLog);
                }

                if (id < 0) break;
                StartCoroutine(ExecutePlan());
                yield return new WaitUntil(() => !hasPlan && active);
            }

            Debug.LogWarning("No se ha encontrado plan asequible" + " | Estado actual: " +
                             _currentState);
        }

        private IEnumerator ExecutePlan()
        {
            PropertyGroup<PropertyList, object> result;
            do
            {
                result = PlanStep(_currentState);
                if (result != null) _currentState = result;
                yield return new WaitWhile(() => performingAction);
            } while (result != null);

            hasPlan = false;
        }

        //INTERFACE CLASSES

        public void AddAction(GoapTFG.Base.Action<PropertyList, object> action)
        {
            _actions.Add(action);
        }

        public void AddActions(List<GoapTFG.Base.Action<PropertyList, object>> actionList)
        {
            _actions.AddRange(actionList);
        }

        public void AddGoal(Goal<PropertyList, object> goal)
        {
            _goals.Add(goal);
            OrderGoals();
        }

        public void AddGoals(List<Goal<PropertyList, object>> goalList)
        {
            _goals.AddRange(goalList);
            OrderGoals();
        }

        public void OrderGoals()
        {
            _goals.Sort((g1, g2) => g2.PriorityLevel.CompareTo(g1.PriorityLevel));
        }

        public int CreateNewPlan(PropertyGroup<PropertyList, object> initialState)
        {
            if (_goals == null || _actions.Count == 0) return -1;
            var i = 0;
            var created = false;
            while (i < _goals.Count && _currentPlan.Count == 0)
            {
                _currentGoal = _goals[i];
                created = CreatePlan(initialState, _currentGoal, GetCustomHeuristic());
                i++;
            }

            if (!created) return -1;
            return i - 1;
        }

        public Goal<PropertyList, object> GetCurrentGoal()
        {
            return _currentGoal;
        }

        public bool CreatePlan(PropertyGroup<PropertyList, object> initialState, Goal<PropertyList, object> goal,
            Func<Goal<PropertyList, object>, PropertyGroup<PropertyList, object>, int> customHeuristic)
        {
            var plan = regressivePlan 
                ? RegressivePlanner<PropertyList, object>.CreatePlan(initialState, goal, _actions, customHeuristic) 
                : Planner<PropertyList, object>.CreatePlan(initialState, goal, _actions, customHeuristic);
            if (plan == null) return false;
            _currentPlan = plan;
            return true;
        }

        public PropertyGroup<PropertyList, object> DoPlan(PropertyGroup<PropertyList, object> worldState)
        {
            if (_currentPlan.Count == 0) return null;

            foreach (var action in _currentPlan)
            {
                worldState = action.PerformAction(worldState);
            }

            _currentPlan.Clear();
            return worldState;
        }

        public PropertyGroup<PropertyList, object> PlanStep(PropertyGroup<PropertyList, object> worldState)
        {
            if (_currentPlan.Count == 0) return null;

            worldState = _currentPlan.Pop().PerformAction(worldState);
            return worldState;
        }

        public int Count()
        {
            return _currentPlan.Count;
        }

        //MOVEMENT RELATED
        public void GoToTarget(string target)
        {
            performingAction = true;
            Blackboard.Target = WorkingMemoryManager.Get(target).Position;
            StartCoroutine(Movement());
        }

        IEnumerator Movement()
        {
            Vector3 finalPos = Blackboard.Target;
            bool reached = false;
            while (!reached)
            {
                var position = transform.position;
                finalPos.y = position.y;
                position = Vector3.MoveTowards(position, finalPos,
                    Time.deltaTime * speed);
                transform.position = position;
                Vector3 aux = finalPos;
                aux.y = position.y;
                if (Vector3.Distance(transform.position, aux) < Single.Epsilon) reached = true;
                yield return new WaitForFixedUpdate();
            }

            performingAction = false;
        }

        public void GoIdleling(float radius)
        {
            performingAction = true;
            speed = 4;
            StartCoroutine(Idleling(radius));
        }
        
        IEnumerator Idleling(float radius)
        {
            float rotation = Random.Range(-270f, 270f);
            var newRot = transform.rotation.eulerAngles;
            newRot.y += rotation;
            newRot.y = Mathf.Clamp(newRot.y, -180, 180);
            transform.rotation = Quaternion.Euler(newRot);
            Vector3 finalPos = Random.Range(radius * 0.25f, radius) * transform.forward;
            bool reached = false;
            while (!reached)
            {
                var position = transform.position;
                finalPos.y = position.y;
                position = Vector3.MoveTowards(position, finalPos,
                    Time.deltaTime * speed);
                transform.position = position;
                Vector3 aux = finalPos;
                aux.y = position.y;
                if (Vector3.Distance(transform.position, aux) < Single.Epsilon) reached = true;
                yield return new WaitForFixedUpdate();
            }
            yield return new WaitForSeconds(2);
            speed = 10;
            performingAction = false;
        }
    }
}