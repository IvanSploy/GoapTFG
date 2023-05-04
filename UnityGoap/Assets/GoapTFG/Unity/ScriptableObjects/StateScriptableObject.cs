using System;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "State", menuName = "Goap Items/State", order = 1)]
    public class StateScriptableObject : ScriptableObject
    {
        [HideInInspector] public List<Property> stateProperties;

        public PropertyGroup<PropertyList, object> Create()
        {
            PropertyGroup<PropertyList, object> state = new();
            AddIntoPropertyGroup(stateProperties, in state);
            return state;
        }
    }
}
