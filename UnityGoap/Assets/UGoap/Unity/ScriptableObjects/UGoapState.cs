using System.Collections.Generic;
using UGoap.Base;
using UnityEngine;
using UnityEngine.Serialization;
using static UGoap.Unity.UGoapPropertyManager;

namespace UGoap.Unity.ScriptableObjects
{
    [CreateAssetMenu(fileName = "State", menuName = "Goap Items/State", order = 1)]
    public class UGoapState : ScriptableObject
    {
        [HideInInspector] public List<Property> properties;

        public GoapState<PropertyKey, object> Create()
        {
            GoapState<PropertyKey, object> goapState = new();
            AddIntoPropertyGroup(properties, in goapState);
            return goapState;
        }
    }
}
