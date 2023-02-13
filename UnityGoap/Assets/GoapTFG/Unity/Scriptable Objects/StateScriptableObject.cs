using System;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.Unity.PropertyManager;

namespace GoapTFG.Unity
{
    [CreateAssetMenu(fileName = "State", menuName = "Goap Items/State", order = 1)]
    public class StateScriptableObject : ScriptableObject
    {
        [HideInInspector] public string stateName;
        [HideInInspector] public List<Property> stateProperties;

        private void OnValidate()
        {
            if(stateName.Equals(""))stateName = name;
        }

        public PropertyGroup<PropertyList, object> Create()
        {
            PropertyGroup<PropertyList, object> state = new();
            AddIntoPropertyGroup(stateProperties, ref state);
            return state;
        }
    }
}
