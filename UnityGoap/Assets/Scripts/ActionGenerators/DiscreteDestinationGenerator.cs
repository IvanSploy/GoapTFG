using System.Collections.Generic;
using LUGoap.Base;
using UnityEngine;
using static LUGoap.Base.BaseTypes;
using static LUGoap.Base.PropertyManager;

[RequireComponent(typeof(IAgent))]
public class DiscreteDestinationGenerator : MonoBehaviour
{
    public bool Active = true;
    
    public Vector3 PositionA;
    public Vector3 PositionB;
    public Vector3Int Count;

    public PropertyKey PropertyX;
    public PropertyKey PropertyY;
    public PropertyKey PropertyZ;

    private IAgent _agent;
    private ConditionGroup _conditionGroup;
    private EffectGroup _effectGroup;

    private void OnValidate()
    {
        Count = Vector3Int.Max(Count, Vector3Int.one);
    }
    
    private void Awake()
    {
        _agent = GetComponent<IAgent>();
        
        _conditionGroup = new ConditionGroup();
        _conditionGroup.SetOrCombine(PropertyKey.MoveState, ConditionType.Equal, "Ready");
        
        _effectGroup = new EffectGroup();
        _effectGroup.Set(PropertyKey.MoveState, EffectType.Set, "Required");

        if(Active) GenerateActions();
    }

    private void GenerateActions()
    {
        List<Action> goapActions = new(); 
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
                    string actionName = "Move";
                    if(PropertyX != PropertyKey.None) actionName += "_" + i;
                    if(PropertyY != PropertyKey.None) actionName += "_" + j;
                    if(PropertyZ != PropertyKey.None) actionName += "_" + k;
                    var action = CreateAction(actionName, x, y, z);
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

    private Action CreateAction(string actionName, float x, float y, float z)
    {
        var effects = new EffectGroup(_effectGroup);
        
        if(PropertyX != PropertyKey.None) effects.Set(PropertyX, EffectType.Set, x);
        if(PropertyY != PropertyKey.None) effects.Set(PropertyY, EffectType.Set, y);
        if(PropertyZ != PropertyKey.None) effects.Set(PropertyZ, EffectType.Set, z);
            
        var action = new SetDestinationAction();
        action.Initialize(actionName, _conditionGroup, effects, _agent);
        return action;
    }
}
