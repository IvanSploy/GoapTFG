using System.Collections.Generic;
using UGoap.Base;
using UGoap.Unity.Actions;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Base.UGoapPropertyManager;

[RequireComponent(typeof(IGoapAgent))]
public class DiscreteDestinationGenerator : MonoBehaviour
{
    public Vector3 PositionA;
    public Vector3 PositionB;
    public Vector3Int Count;

    public PropertyKey PropertyX;
    public PropertyKey PropertyY;
    public PropertyKey PropertyZ;

    private IGoapAgent _agent;
    private GoapConditions _conditions;
    private GoapEffects _effects;

    private void OnValidate()
    {
        Count = Vector3Int.Max(Count, Vector3Int.one);
    }
    
    private void Awake()
    {
        _agent = GetComponent<IGoapAgent>();
        
        _conditions = new GoapConditions();
        _conditions.Set(PropertyKey.MoveState, ConditionType.Equal, "Ready");
        
        _effects = new GoapEffects();
        _effects.Set(PropertyKey.MoveState, EffectType.Set, "Set");

        GenerateActions();
    }

    private void GenerateActions()
    {
        List<IGoapAction> goapActions = new(); 
        var configX = DimensionConfig(PositionA.x, PositionB.x, Count.x);
        var configY = DimensionConfig(PositionA.y, PositionB.y, Count.y);
        var configZ = DimensionConfig(PositionA.z, PositionB.z, Count.z);
        
        for (int i = 0; i < Count.x; i++)
        {
            var x = configX.initial + configX.distance * i;
            for (int j = 0; j < Count.y; j++)
            {
                var y = configY.initial + configY.distance * j;
                for (int k = 0; k < Count.z; k++)
                {
                    var z = configZ.initial + configZ.distance * k;
                    var action = CreateAction("Destination_" + i + "_" + j + "_" + k, x, y, z);
                    goapActions.Add(action);
                }
            }
        }
        
        _agent.AddActions(goapActions);
    }

    private (float initial, float distance) DimensionConfig(float a, float b, int count)
    {
        float initial = a;
        float distance = 0;
        if (Count.x <= 1)   initial = (a + b) * 0.5f;
        else                distance = (b - a) / (count - 1);
        return (initial, distance);
    }

    private IGoapAction CreateAction(string actionName, float x, float y, float z)
    {
        var effects = new GoapEffects(_effects);
        
        if(PropertyX != PropertyKey.None) effects.Set(PropertyX, EffectType.Set, x);
        if(PropertyY != PropertyKey.None) effects.Set(PropertyY, EffectType.Set, y);
        if(PropertyZ != PropertyKey.None) effects.Set(PropertyZ, EffectType.Set, z);
            
        return new SetDestinationAction(actionName, _conditions, effects);
    }
}