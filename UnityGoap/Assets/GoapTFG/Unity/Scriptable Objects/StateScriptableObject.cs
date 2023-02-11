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
        [HideInInspector] public string nameItem;
        [HideInInspector] public List<Property> stateProperties;
    
        private void Awake()
        {
            nameItem = name;
        }

        private void OnValidate()
        {
            if(nameItem.Equals(""))nameItem = name;
        }

        public PropertyGroup<string, object> Create()
        {
            PropertyGroup<string, object> state = new();
            AddIntoPropertyGroup(stateProperties, ref state);
            return state;
        }
    }
}
