using System;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using UnityEngine.Events;
using static PropertyManager;

[CreateAssetMenu(fileName = "GoapItem", menuName = "ScriptableObjects/GoapItem", order = 1)]
public class GoapItem : ScriptableObject
{
    public enum ItemType
    {
        State,
        Goal,
        Action
    }

    [Header("Goap Item")] public ItemType type;

    [HideInInspector] public string nameItem;

    //State
    [HideInInspector] public List<Property> stateProperties;

    //Goal
    [HideInInspector] public List<Property> goalProperties;
    [HideInInspector] public float priority;

    //Action
    [HideInInspector] public List<Property> preconditions;
    [HideInInspector] public List<Property> effects;
    [HideInInspector] public int cost;
    
    private void Awake()
    {
        nameItem = name;
        cost = 1;
    }

    private void OnValidate()
    {
        cost = Math.Max(1, cost);
    }

    public PropertyGroup<string, object> GetState()
    {
        if (type != ItemType.State) throw new Exception("Tried to extract a different type of GoapItem that required. Try changing the type or using another Get.");
        PropertyGroup<string, object> state = new();
        ConvertToPropertyGroup(stateProperties, ref state);
        return state;
    }
    
    public Goal<string, object> GetGoal()
    {
        if (type != ItemType.Goal) throw new Exception("Tried to extract a different type of GoapItem that required. Try changing the type or using another Get.");
        PropertyGroup<string, object> state = new();
        ConvertToPropertyGroup(stateProperties, ref state);
        return new Goal<string, object>(nameItem, state, priority);
    }
    
    public GoapTFG.Base.Action<string, object> GetAction()
    {
        if (type != ItemType.Action) throw new Exception("Tried to extract a different type of GoapItem that required. Try changing the type or using another Get.");
        PropertyGroup<string, object> precsPG = new();
        ConvertToPropertyGroup(preconditions, ref precsPG);
        PropertyGroup<string, object> effectsPG = new();
        ConvertToPropertyGroup(this.effects, ref effectsPG);
        GoapTFG.Base.Action<string, object> action = new(nameItem, precsPG, effectsPG)
        {
            Cost = cost
        };
        ActionAdditionalData data = GoapData.GetActionAdditionalData(nameItem);
        if (data != null)
        {
            action.ProceduralConditions += data.conditions;
            action.ProceduralEffects += data.effects;
            action.PerformedActions += data.actions;
        }
        return action;
    }

    private void ConvertToPropertyGroup(List<Property> properties, ref PropertyGroup<string, object> state)
    {
        foreach (var property in properties)
        {
            ApplyProperty(property, ref state);
        }
    }
}
