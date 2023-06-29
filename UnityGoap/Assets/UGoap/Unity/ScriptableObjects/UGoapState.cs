using System;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using UnityEngine.Serialization;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap.ScriptableObjects
{
    [CreateAssetMenu(fileName = "State", menuName = "Goap Items/State", order = 1)]
    public class UGoapState : ScriptableObject
    {
        [FormerlySerializedAs("stateProperties")] [HideInInspector] public List<Property> properties;

        public PropertyGroup<PropertyKey, object> Create()
        {
            PropertyGroup<PropertyKey, object> state = new();
            AddIntoPropertyGroup(properties, in state);
            return state;
        }
    }
}
