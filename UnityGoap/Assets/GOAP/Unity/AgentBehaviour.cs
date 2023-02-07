using System;
using System.Collections;
using System.Collections.Generic;
using GoapTFG.Base;
using GoapTFG.Planner;
using JetBrains.Annotations;
using UnityEngine;

public class AgentBehaviour : MonoBehaviour
{
    //Propiedades
    public PropertyManager.Property[] GoalValues;
    public float PriorityLevel;
    public bool Enabled = true;
    public bool HasPlan = false;

    //Referencias
    [CanBeNull] public Agent<string, object> agent;

    // Start is called before the first frame update
    void Awake()
    {
        PropertyGroup<string, object> pg = new PropertyGroup<string, object>();
        foreach (var property in GoalValues)
        {
            PropertyManager.ApplyProperty(property, ref pg);
        }

        Goal<string, object> myGoal = new Goal<string, object>(pg, PriorityLevel);

        PropertyGroup<string, object> pgPrec = new PropertyGroup<string, object>();
        PropertyGroup<string, object> pgEffect = new PropertyGroup<string, object>();

        pgPrec.Set(PropertyManager.PropertyList.IsAlive.ToString(), true);
        pgPrec.Set(PropertyManager.PropertyList.GoldCount.ToString(), 0);
        
        pgEffect.Set(PropertyManager.PropertyList.GoldCount.ToString(), 100);

        GoapTFG.Base.Action<string, object> mainAction =
            new GoapTFG.Base.Action<string, object>("GoldReward", pgPrec, pgEffect);
        
        agent = new Agent<string, object>(myGoal);
        agent.AddAction(mainAction);
    }

    private void Start()
    {
        StartCoroutine(Replan());
    }

    private IEnumerator Replan()
    {
        while (true)
        {
            var id = agent.UpdateBehaviour(GoapData.Instance.actualState);
            if (id >= 0) HasPlan = true;
            if (id < 0) break;
            StartCoroutine(ExecutePlan());
            yield return new WaitUntil(() => !HasPlan && Enabled);
        }
        Debug.LogWarning("No se ha encontrado plan asesquible" + " | Estado actual: " + GoapData.Instance.actualState);
    }

    private IEnumerator ExecutePlan()
    {
        PropertyGroup<string, object> result;
        do
        {
            result = agent.PlanStep(GoapData.Instance.actualState);
            if (result != null) GoapData.Instance.actualState = result;
            yield return new WaitForEndOfFrame();
        } while (result != null);

        HasPlan = false;
    }
}
