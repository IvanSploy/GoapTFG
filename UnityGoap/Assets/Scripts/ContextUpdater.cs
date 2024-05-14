using System;
using System.Collections.Generic;
using UGoap.Base;
using UGoap.Unity.Actions;
using UnityEngine;
using static UGoap.Base.BaseTypes;
using static UGoap.Base.UGoapPropertyManager;

[RequireComponent(typeof(IGoapAgent))]
public class ContextUpdater : MonoBehaviour
{
    public bool Active = true;

    public GameObject Target;

    public PropertyKey PropertyX;
    public PropertyKey PropertyY;
    public PropertyKey PropertyZ;

    private IGoapAgent _agent;

    private void Awake()
    {
        _agent = GetComponent<IGoapAgent>();
    }

    private void Update()
    {
        if (Active)
        {
            var position = Target.transform.position;
            var x = Mathf.Round(position.x);
            var y = Mathf.Round(position.y);
            var z = Mathf.Round(position.z);
        
            if(PropertyX != PropertyKey.None) _agent.CurrentState.Set(PropertyX, x);
            if(PropertyY != PropertyKey.None) _agent.CurrentState.Set(PropertyY, y);
            if(PropertyZ != PropertyKey.None) _agent.CurrentState.Set(PropertyZ, z);
        }
    }
}
