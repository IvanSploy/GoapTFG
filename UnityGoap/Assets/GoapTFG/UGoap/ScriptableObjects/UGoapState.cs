using System;
using System.Collections.Generic;
using GoapTFG.Base;
using UnityEngine;
using static GoapTFG.UGoap.UGoapPropertyManager;

namespace GoapTFG.UGoap.ScriptableObjects
{
    [CreateAssetMenu(fileName = "State", menuName = "Goap Items/State", order = 1)]
    public class UGoapState : ScriptableObject
    {
        [HideInInspector] public List<UGoapPropertyManager.Property> stateProperties;

        public PropertyGroup<UGoapPropertyManager.PropertyList, object> Create()
        {
            PropertyGroup<UGoapPropertyManager.PropertyList, object> state = new();
            AddIntoPropertyGroup(stateProperties, in state);
            return state;
        }
    }
}
