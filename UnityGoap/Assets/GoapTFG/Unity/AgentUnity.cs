using System;
using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Planner;
using GoapTFG.Unity;
using UnityEngine;
using static GoapData;

public class AgentUnity : MonoBehaviour, IAgent<string, object>
{
    [Serializable]
    private struct GoalObject
    {
        [SerializeField]
        private GoalScriptableObject goal;
        
        [Range(0, 15)]
        [SerializeField]
        private int priority;

        public Goal<string, object> Create()
        {
            return goal.Create(priority);
        }
    }
    
    [SerializeField]
    private List<GoalObject> goals;
    public List<ActionScriptableObject> actions;
    public bool active = true;
    public bool hasPlan;
    public bool performingAction = false;
    public float speed = 5;
    
    //Agent base related
    private List<GoapTFG.Base.Action<string, object>> _currentPlan;
    private List<Goal<string, object>> _goals;
    private List<GoapTFG.Base.Action<string, object>> _actions;
    
    //Referencias
    public BlackboardData Blackboard;

    // Start is called before the first frame update
    void Start()
    {
        _currentPlan = new();
        _goals = new();
        _actions = new();
        List<Goal<string, object>> myGoals = new();
        List<GoapTFG.Base.Action<string, object>> myActions = new();
            
        //OBJETIVOS
        foreach (var goal in goals)
        {
            _goals.Add(goal.Create());
        }
        
        //ACCION PRINCIPAL
        foreach (var action in actions)
        {
            _actions.Add(action.Create(this));
        }
        OrderGoals();
        
        //Se crea el blackboard utilizado por las acciones de GOAP.
        Blackboard = new BlackboardData();
        
        //Comienza la planificación
        if (GoapDataInstance == null)
        {
            Debug.LogError("GoapData necesario para utilizar Agentes de Goap");
            throw new Exception();
        }
        StartCoroutine(Replan());
    }

    //CORRUTINAS
    private IEnumerator Replan()
    {
        while (true)
        {
            var id = CreateNewPlan(GoapDataInstance.actualState);
            if (id >= 0) hasPlan = true;
            if (id < 0) break;
            StartCoroutine(ExecutePlan());
            yield return new WaitUntil(() => !hasPlan && active);
        }
        Debug.LogWarning("No se ha encontrado plan asequible" + " | Estado actual: " + GoapDataInstance.actualState);
    }

    private IEnumerator ExecutePlan()
    {
        PropertyGroup<string, object> result;
        do
        {
            result = PlanStep(GoapDataInstance.actualState);
            if (result != null) GoapDataInstance.actualState = result;
            yield return new WaitWhile(() => performingAction);
        } while (result != null);

        hasPlan = false;
    }

    //INTERFACE CLASSES
    
    public void AddAction(GoapTFG.Base.Action<string, object> action)
    {
        _actions.Add(action);
    }

    public void AddActions(List<GoapTFG.Base.Action<string, object>> actions)
    {
        _actions.AddRange(actions);
    }

    public void AddGoal(Goal<string, object> goal)
    {
        _goals.Add(goal);
        OrderGoals();
    }

    public void AddGoals(List<Goal<string, object>> goals)
    {
        _goals.AddRange(goals);
        OrderGoals();
    }

    public void OrderGoals()
    {
        _goals.Sort((g1, g2) => g2.PriorityLevel.CompareTo(g1.PriorityLevel));
    }

    public int CreateNewPlan(PropertyGroup<string, object> initialState)
    {
        if (_goals == null || _actions.Count == 0) return -1;
        var i = 0;
        var created = false;
        while (i < _goals.Count && _currentPlan.Count == 0)
        {
            created = CreatePlan(initialState, _goals[i]);
            i++;
        }

        if (!created) return -1;
        return i-1;
    }

    public bool CreatePlan(PropertyGroup<string, object> initialState, Goal<string, object> goal)
    {
        var plan = NodeGenerator<string, object>.CreatePlan(initialState, goal, _actions);
        if (plan == null) return false;
        _currentPlan = plan;
        return true;
    }

    public PropertyGroup<string, object> DoPlan(PropertyGroup<string, object> worldState)
    {
        if (_currentPlan.Count == 0) return null;

        foreach (var action in _currentPlan)
        {
            worldState = action.PerformAction(worldState);
        }

        _currentPlan.Clear();
        return worldState;
    }

    public PropertyGroup<string, object> PlanStep(PropertyGroup<string, object> worldState)
    {
        if (_currentPlan.Count == 0 ) return null;

        worldState = _currentPlan[0].PerformAction(worldState);
        _currentPlan.RemoveAt(0);
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
        Blackboard.Target = WorkingMemoryManager.Get(target).Position.Value;
        StartCoroutine(Movement());
    }

    IEnumerator Movement()
    {
        Vector3 finalPos = Blackboard.Target;
        bool reached = false;
        while (!reached)
        {
            var position = transform.position;
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
}