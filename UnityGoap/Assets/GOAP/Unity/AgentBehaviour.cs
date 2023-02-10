using System;
using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Planner;
using JetBrains.Annotations;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static GoapData;
using static PropertyManager;
using static PropertyManager.PropertyList;

public class AgentBehaviour : MonoBehaviour
{
    //Propiedades
    public GoapItem goal;
    public GoapItem action;
    public float priorityLevel;
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
        Goal<string, object> myGoal = goal.GetGoal();
        
        //Condiciones especiales, sirven para reemplazar la instrucción equals.
        myGoal.SetPredicate(GoldCount.ToString(), (a, b) => (float)a >= (float)b);

        //ACCION PRINCIPAL
        GoapTFG.Base.Action<string, object> myAction = action.GetAction();
        myAction.SetPredicate(GoldCount.ToString(), (a, b) => (float)a <= (float)b);

        //ACCION GOTOTARGET
        PropertyGroup<string, object> pgPrec = new PropertyGroup<string, object>();
        PropertyGroup<string, object> pgEffect = new PropertyGroup<string, object>();
        
        ApplyProperty(new Property(IsAlive, "true"), ref pgPrec);
        ApplyProperty(new Property(Target, "Hall"), ref pgEffect);
        
        GoapTFG.Base.Action<string, object> goToHall =
            new GoapTFG.Base.Action<string, object>("GoToHall", pgPrec, pgEffect);

        goToHall.PerformedActions += (ws) => GoToTarget((string)ws.Get(Target.ToString()));
        
        ApplyProperty(new Property(Target, "Mine"), ref pgEffect);
        
        GoapTFG.Base.Action<string, object> goToMine =
            new GoapTFG.Base.Action<string, object>("GoToMine", pgPrec, pgEffect);

        goToMine.PerformedActions += (ws) => GoToTarget((string)ws.Get(Target.ToString()));
        
        ApplyProperty(new Property(Target, "Cottage"), ref pgEffect);
        
        GoapTFG.Base.Action<string, object> goToCottage =
            new GoapTFG.Base.Action<string, object>("GoToCottage", pgPrec, pgEffect);

        goToCottage.PerformedActions += (ws) => GoToTarget((string)ws.Get(Target.ToString()));
        
        Agent = new Agent<string, object>(myGoal);
        Agent.AddAction(myAction);
        Agent.AddAction(goToHall);
        Agent.AddAction(goToMine);
        Agent.AddAction(goToCottage);

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
