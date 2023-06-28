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
    public class GoapAgent : MonoBehaviour, IGoapAgent<PropertyList, object>
    {
        [SerializeField] private List<GoapPriorityGoalSO> goalObjects;
        [SerializeField] private List<BaseGoapAction> actionObjects;
        
        public string Name { get; set; }
        
        public bool active = true;
        public bool hasPlan;
        public bool performingAction = false;
        public bool regressivePlan = false;
        public float speed = 5;

        //Agent base related
        private Stack<IGoapAction<PropertyList, object>> _currentPlan;
        private List<GoapGoal<PropertyList, object>> _goals;
        private List<IGoapAction<PropertyList, object>> _actions;
        private GoapGoal<PropertyList, object> _currentGoapGoal;
        
        public PropertyGroup<PropertyList, object> CurrentState { get; set; }

        // Start is called before the first frame update
        void Start()
        {
            _currentPlan = new();
            _goals = new();
            _actions = new();
            List<GoapGoal<PropertyList, object>> myGoals = new();
            List<IGoapAction<PropertyList, object>> myActions = new();
            CurrentState = new();
            
            //OBJETIVOS
            foreach (var goal in goalObjects)
            {
                _goals.Add(goal.Create());
            }

            //ACCIONES
            foreach (var action in actionObjects)
            {
                _actions.Add(action.Clone());
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
                Debug.Log("Estado actual: " + CurrentState);
                var id = CreateNewPlan(CurrentState);
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
                             CurrentState);
        }

        private IEnumerator ExecutePlan()
        {
            PropertyGroup<PropertyList, object> result;
            do
            {
                result = PlanStep(CurrentState);
                if (result != null) CurrentState = result;
                yield return new WaitWhile(() => performingAction);
            } while (result != null);

            hasPlan = false;
        }

        //INTERFACE CLASSES

        public void AddAction(IGoapAction<PropertyList, object> goapAction)
        {
            _actions.Add(goapAction);
        }

        public void AddActions(List<IGoapAction<PropertyList, object>> actionList)
        {
            _actions.AddRange(actionList);
        }

        public void AddGoal(GoapGoal<PropertyList, object> goapGoal)
        {
            _goals.Add(goapGoal);
            SortGoals();
        }

        public void AddGoals(List<GoapGoal<PropertyList, object>> goalList)
        {
            _goals.AddRange(goalList);
            SortGoals();
        }

        public void SortGoals()
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
                _currentGoapGoal = _goals[i];
                created = CreatePlan(initialState, _currentGoapGoal, GetCustomHeuristic());
                i++;
            }

            if (!created) return -1;
            return i - 1;
        }

        public GoapGoal<PropertyList, object> GetCurrentGoal()
        {
            return _currentGoapGoal;
        }

        public bool CreatePlan(PropertyGroup<PropertyList, object> initialState, GoapGoal<PropertyList, object> goapGoal,
            Func<GoapGoal<PropertyList, object>, PropertyGroup<PropertyList, object>, int> customHeuristic)
        {
            var plan = regressivePlan 
                ? RegressivePlanner<PropertyList, object>.CreatePlan(initialState, goapGoal, _actions, customHeuristic) 
                : Planner<PropertyList, object>.CreatePlan(initialState, goapGoal, _actions, customHeuristic);
            if (plan == null) return false;
            _currentPlan = plan;
            return true;
        }

        public PropertyGroup<PropertyList, object> DoPlan(PropertyGroup<PropertyList, object> worldState)
        {
            if (_currentPlan.Count == 0) return null;

            foreach (var action in _currentPlan)
            {
                worldState = action.Execute(worldState, this);
            }

            _currentPlan.Clear();
            return worldState;
        }

        public PropertyGroup<PropertyList, object> PlanStep(PropertyGroup<PropertyList, object> worldState)
        {
            if (_currentPlan.Count == 0) return null;

            worldState = _currentPlan.Pop().Execute(worldState, this);
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
            StartCoroutine(Movement(WorkingMemoryManager.Get(target).Position));
        }

        private IEnumerator Movement(Vector3 finalPos)
        {
            Debug.Log("Moviendo a " + finalPos);
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
                yield return null;
            }

            Debug.Log("Llegado a " + finalPos);
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
                yield return null;
            }
            yield return new WaitForSeconds(2);
            speed = 10;
            performingAction = false;
        }
    }
}