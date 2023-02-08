using System;
using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Planner;
using JetBrains.Annotations;
using UnityEngine;
using static GoapData;
using static PropertyManager;
using static PropertyManager.PropertyList;

public class AgentBehaviour : MonoBehaviour
{
    //Propiedades
    public Property[] goalValues;
    public float priorityLevel;
    public bool active = true;
    public bool hasPlan;
    public bool performingAction = false;
    public float speed = 5;

    //Referencias
    public Agent<string, object> Agent;

    // Start is called before the first frame update
    void Awake()
    {
        PropertyGroup<string, object> goalState = new PropertyGroup<string, object>();
        foreach (var property in goalValues)
        {
            ApplyProperty(property, ref goalState);
        }

        //Condiciones especiales, sirven para reemplazar la instrucción equals.
        goalState.SetPredicate(GoldCount.ToString(), (a, b) => (float)a >= (float)b);
        
        Goal<string, object> myGoal = new Goal<string, object>(goalState, priorityLevel);

        //ACCION PRINCIPAL
        PropertyGroup<string, object> pgPrec = new PropertyGroup<string, object>();
        PropertyGroup<string, object> pgEffect = new PropertyGroup<string, object>();

        //Condiciones estáticas con predicados personalizados.
        ApplyProperty(new Property(GoldCount, "50"), ref pgPrec, (a,b) => (float)a <= (float)b);
        ApplyProperty(new Property(Target, "Mine"), ref pgPrec);
        ApplyProperty(new Property(IsAlive, "true"), ref pgPrec);
        
        //ApplyProperty(new Property(GoldCount, "150"), ref pgEffect);

        GoapTFG.Base.Action<string, object> mainAction =
            new GoapTFG.Base.Action<string, object>("GoldReward", pgPrec, pgEffect);

        //Posible apliacación de efectos procedurales.
        //mainAction.ProceduralConditions += (pg) => (float) pg.Get(GoldCount.ToString()) <= 50f;
        mainAction.ProceduralEffects += (pg) => pg.Set(GoldCount.ToString(),
            (float) pg.Get(GoldCount.ToString()) + 100f);
        
        //ACCION GOTOTARGET
        pgPrec = new PropertyGroup<string, object>();
        pgEffect = new PropertyGroup<string, object>();
        
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
        Agent.AddAction(mainAction);
        Agent.AddAction(goToHall);
        Agent.AddAction(goToMine);
        Agent.AddAction(goToCottage);
    }

    private void Start()
    {
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
        MemoryFact<Vector3, GameObject> fact = BlackboardData.Get(target);
        StartCoroutine(Movement(fact.Position));
    }

    IEnumerator Movement(Vector3 finalPos)
    {
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
            yield return new WaitForEndOfFrame();
        }
        performingAction = false;
    }
}
