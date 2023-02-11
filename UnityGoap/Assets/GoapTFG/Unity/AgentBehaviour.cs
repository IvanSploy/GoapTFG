using System;
using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Planner;
using GoapTFG.Unity;
using JetBrains.Annotations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static GoapData;
using static GoapTFG.Unity.PropertyManager;
using static GoapTFG.Unity.PropertyManager.PropertyList;



public class AgentBehaviour : MonoBehaviour
{
    [Serializable]
    private struct GoalObject
    {
        [SerializeField]
        private GoalScriptableObject goal;
        
        [Range(20, 0)]
        [SerializeField]
        private int priority;

        public Goal<string, object> Create()
        {
            return goal.Create(priority);
        }
    }
    [SerializeField]
    private GoalObject goal;
    public List<ActionScriptableObject> actions;
    public bool active = true;
    public bool hasPlan;
    public bool performingAction = false;
    public float speed = 5;

    //Referencias
    public Agent<string, object> Agent;
    public BlackboardData Blackboard;

    // Start is called before the first frame update
    void Start()
    {
        Goal<string, object> myGoal = goal.Create();
        List<GoapTFG.Base.Action<string, object>> myActions = new();
            
        //ACCION PRINCIPAL
        foreach (var action in actions)
        {
            myActions.Add(action.Create());
        }
        
        Agent = new Agent<string, object>(myGoal, myActions);

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

    private IEnumerator Replan()
    {
        while (true)
        {
            var id = Agent.UpdateBehaviour(GoapDataInstance.actualState);
            if (id >= 0) hasPlan = true;
            if (id < 0) break;
            StartCoroutine(ExecutePlan());
            yield return new WaitUntil(() => !hasPlan && active);
        }
        Debug.LogWarning("No se ha encontrado plan asesquible" + " | Estado actual: " + GoapDataInstance.actualState);
    }

    private IEnumerator ExecutePlan()
    {
        PropertyGroup<string, object> result;
        do
        {
            result = Agent.PlanStep(GoapDataInstance.actualState);
            if (result != null) GoapDataInstance.actualState = result;
            yield return new WaitWhile(() => performingAction);
        } while (result != null);

        hasPlan = false;
    }

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
